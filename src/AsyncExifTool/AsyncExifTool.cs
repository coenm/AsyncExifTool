namespace CoenM.ExifToolLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals;
    using CoenM.ExifToolLib.Internals.AsyncManualResetEvent;
    using CoenM.ExifToolLib.Internals.MedallionShell;
    using CoenM.ExifToolLib.Internals.Stream;
    using CoenM.ExifToolLib.Logging;

    using JetBrains.Annotations;
    using Nito.AsyncEx;

    public class AsyncExifTool
#if FEATURE_ASYNC_DISPOSABLE
        : IAsyncDisposable
#else
        : IDisposable
#endif
    {
        private readonly string exifToolPath;
        private readonly AsyncLock executeAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock executeImpAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock disposingSyncLock = new AsyncLock();
        private readonly object cmdExitedSubscribedSyncLock = new object();
        private readonly object initializedSyncLock = new object();
        private readonly CancellationTokenSource stopQueueCts;

        private readonly List<string> exifToolArguments;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> waitingTasks;
        private readonly ExifToolStayOpenStream stream;
        private readonly AsyncManualResetEvent cmdExitedMre;
        private readonly ILogger logger;
        private IShell shell;
        private int key;
        private bool disposed;
        private bool disposing;
        private bool cmdExited;
        private bool cmdExitedSubscribed;
        private bool initialized;


        /// <summary>
        /// Construct AsyncExifTool with configuration and without a logger.
        /// </summary>
        /// <param name="configuration">Configuration for Exiftool</param>
        public AsyncExifTool([NotNull] AsyncExifToolConfiguration configuration)
            : this (configuration, NullLogger.Instance)
        {
        }

        /// <summary>
        /// Construct AsyncExifTool with configuration and a logger instance.
        /// </summary>
        /// <param name="configuration">Configuration for Exiftool</param>
        /// <param name="logger">The logger.</param>
        public AsyncExifTool([NotNull] AsyncExifToolConfiguration configuration, [NotNull] ILogger logger)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            stream = new ExifToolStayOpenStream(configuration.ExifToolEncoding, configuration.ExifToolNewLine);
            stopQueueCts = new CancellationTokenSource();
            initialized = false;
            disposed = false;
            disposing = false;
            cmdExited = false;
            cmdExitedMre = new AsyncManualResetEvent(false);
            key = 0;
            exifToolPath = configuration.ExifToolFullFilename;
            exifToolArguments = new List<string>
                {
                    ExifToolArguments.StayOpen,
                    ExifToolArguments.BoolTrue,
                    "-@", // read from argument file
                    "-", // argument file is std in
                }
                .Concat(configuration.CommonArgs.ToList())
                .ToList();

            waitingTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        }

        public void Initialize()
        {
            if (initialized)
                return;

            lock (initializedSyncLock)
            {
                if (initialized)
                    return;

                logger.Info("Initializing");

                stream.Update += StreamOnUpdate;

                shell = CreateShell(exifToolPath, exifToolArguments, stream, null);

                // possible race condition.. to fix
                shell.ProcessExited += ShellOnProcessExited;

                cmdExitedSubscribed = true;
                initialized = true;

                logger.Info("Initialized");
            }
        }

        public async Task<string> ExecuteAsync(IEnumerable<string> args, CancellationToken ct = default)
        {
            if (!initialized)
                throw new Exception("Not initialized");
            if (disposed)
                throw new Exception("Disposed");
            if (disposing)
                throw new Exception("Disposing");

            logger.Debug($"{nameof(ExecuteAsync)} - Wait before entering {nameof(executeAsyncSyncLock)}.");
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, stopQueueCts.Token);
            using (await executeAsyncSyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
            {
                logger.Debug($"{nameof(ExecuteAsync)} - Entered {nameof(executeAsyncSyncLock)}.");
                var result = await ExecuteImpAsync(args, ct).ConfigureAwait(false);
                logger.Debug($"{nameof(ExecuteAsync)} - Released {nameof(executeAsyncSyncLock)}.");
                return result;
            }
        }

        public async Task DisposeAsync(CancellationToken ct = default)
        {
            if (!initialized)
                return;

            if (disposed)
                return;

            using (await disposingSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                if (disposed)
                    return;

                disposing = true;

                // cancel all requests to process.
                stopQueueCts?.Cancel();

                var timeout = TimeSpan.FromMilliseconds(100);

                if (!cmdExited)
                {
                    // This is really not okay. Not sure why or when the stay-open False command doesn't seem to work.
                    // This is just a stupid 'workaround' and is okay for now.
                    await cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                }

                if (!cmdExited)
                {
                    // Try quit ExifTool process using '-stay_open' 'false' arguments.
                    var command = new[] { ExifToolArguments.StayOpen, ExifToolArguments.BoolFalse };

                    // todo ct can be cancelled..
                    await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);

                    await cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                }

                if (!cmdExited)
                {
                    // Try quit ExifTool process by sending Ctrl-C to the process.
                    // This does not always work (depending on the OS, and if the process runs in a console or not).
                    await shell.TryCancelAsync().ConfigureAwait(false);
                    await cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                }

                if (!cmdExited)
                {
                    // Try kill the process.
                    try
                    {
                        shell.Kill();
                        await cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception happened during kill {e.Message}");
                    }
                }

                stream.Update -= StreamOnUpdate;
                UnsubscribeCmdOnProcessExitedOnce();
                Ignore(() => stream.Dispose());
                shell = null;
                disposed = true;
                disposing = false;
            }
        }

# if FEATURE_ASYNC_DISPOSABLE
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(CancellationToken.None).ConfigureAwait(false);
        }
#else
        public void Dispose()
        {
            DisposeAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
#endif

        internal virtual IShell CreateShell(string exifToolFullPath, IEnumerable<string> args, Stream outputStream, Stream errorStream)
        {
            return new MedallionShellAdapter(exifToolFullPath, args, outputStream, errorStream);
        }

        private static void Ignore(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private async Task<string> ExecuteImpAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                var tcs = new TaskCompletionSource<string>();

#if FEATURE_ASYNC_DISPOSABLE
                await using (ct.Register(() => tcs.TrySetCanceled()))
                {
                    key++;
                    var keyString = key.ToString();

                    if (!waitingTasks.TryAdd(keyString, tcs))
                        throw new Exception("Could not execute");

                    await AddToExifToolAsync(keyString, args).ConfigureAwait(false);
                    return await tcs.Task.ConfigureAwait(false);
                }
#else
                using (ct.Register(() => tcs.TrySetCanceled()))
                {
                    key++;
                    var keyString = key.ToString();

                    if (!waitingTasks.TryAdd(keyString, tcs))
                        throw new Exception("Could not execute");

                    await AddToExifToolAsync(keyString, args).ConfigureAwait(false);
                    return await tcs.Task.ConfigureAwait(false);
                }
#endif
            }
        }

        private async Task ExecuteOnlyAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                await AddToExifToolAsync(null, args).ConfigureAwait(false);
            }
        }

        private async Task AddToExifToolAsync(string executeKey, [NotNull] IEnumerable<string> args)
        {
            foreach (var arg in args)
                await shell.WriteLineAsync(arg).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(executeKey))
                await shell.WriteLineAsync($"-execute{executeKey}").ConfigureAwait(false);
        }

        private void StreamOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            if (!waitingTasks.TryRemove(dataCapturedArgs.Key, out var tcs))
                return;

            tcs.TrySetResult(dataCapturedArgs.Data);
        }

        private void ShellOnProcessExited(object sender, EventArgs eventArgs)
        {
            cmdExited = true;
            cmdExitedMre.Set();
            UnsubscribeCmdOnProcessExitedOnce();
        }

        private void UnsubscribeCmdOnProcessExitedOnce()
        {
            if (!cmdExitedSubscribed)
                return;

            lock (cmdExitedSubscribedSyncLock)
            {
                if (!cmdExitedSubscribed)
                    return;

                cmdExitedSubscribed = false;
                shell.ProcessExited -= ShellOnProcessExited;
            }
        }
    }
}
