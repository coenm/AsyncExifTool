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
    using CoenM.ExifToolLib.Internals.Guards;
    using CoenM.ExifToolLib.Internals.MedallionShell;
    using CoenM.ExifToolLib.Internals.Stream;
    using CoenM.ExifToolLib.Internals.TimeoutExtensions;
    using CoenM.ExifToolLib.Logging;
    using JetBrains.Annotations;
    using Nito.AsyncEx;

    /// <summary>
    /// AsyncExifTool is a wrapper around an ExifTool process for executing commands using the `stay-open` flag.
    /// </summary>
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
        private readonly ExifToolStdOutWriter stream;
        private readonly ExifToolStdErrWriter errorStream;
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
        /// <param name="configuration">Configuration for Exiftool.</param>
        public AsyncExifTool([NotNull] AsyncExifToolConfiguration configuration)
            : this(configuration, new NullLogger())
        {
        }

        /// <summary>
        /// Construct AsyncExifTool with configuration and a logger instance.
        /// </summary>
        /// <param name="configuration">Configuration for Exiftool.</param>
        /// <param name="logger">The logger.</param>
        public AsyncExifTool([NotNull] AsyncExifToolConfiguration configuration, [NotNull] ILogger logger)
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(logger, nameof(logger));

            this.logger = logger;
            stream = new ExifToolStdOutWriter(configuration.ExifToolEncoding, configuration.ExifToolNewLine);
            errorStream = new ExifToolStdErrWriter(configuration.ExifToolEncoding);
            stopQueueCts = new CancellationTokenSource();
            initialized = false;
            disposed = false;
            disposing = false;
            cmdExited = false;
            cmdExitedMre = new AsyncManualResetEvent(false);
            key = 0;
            exifToolPath = configuration.ExifToolFullFilename;
            exifToolArguments = new List<string>()
                .Concat(ExifToolArguments.StayOpenMode(true))
                .Concat(ExifToolArguments.ReadCommandLineArgumentsFromStdIn())
                .Concat(configuration.CommonArgs)
                .ToList();

            waitingTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        }

        /// <summary>
        /// Initialize <see cref="AsyncExifTool"/>. This will start the ExifTool process on the host.
        /// </summary>
        /// <exception cref="AsyncExifToolInitialisationException">Thrown when the ExifTool process throws an exception when started.</exception>
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
                errorStream.Error += StreamOnError;

                shell = CreateShell(exifToolPath, exifToolArguments, stream, errorStream);
                shell.ProcessExited += ShellOnProcessExited;
                cmdExitedSubscribed = true;

                try
                {
                    shell.Initialize();
                }
                catch (Exception e)
                {
                    shell.ProcessExited -= ShellOnProcessExited;
                    cmdExitedSubscribed = false;

                    throw new AsyncExifToolInitialisationException("Could not initialise exiftool", e);
                }

                initialized = true;

                logger.Info("Initialized");
            }
        }

        /// <summary>
        /// Execute args on the ExifTool process.
        /// </summary>
        /// <param name="args">The arguments to pass to ExifTool.</param>
        /// <param name="ct">CancellationToken to cancel the pending request. If the request is processing, it is not possible to cancel. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>ExifTool response.</returns>
        /// <exception cref="Exception">Thrown when not in the correct state (ie, should be initialized, and not disposing or disposed).</exception>
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

#if FEATURE_ASYNC_DISPOSABLE
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncImplementation().ConfigureAwait(false);
        }
#else
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        ///
        public Task DisposeAsync()
        {
            return DisposeAsyncImplementation();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposeAsyncImplementation().GetAwaiter().GetResult();
        }
#endif

        internal virtual IShell CreateShell(string exifToolFullPath, IEnumerable<string> args, [NotNull] ExifToolStdOutWriter exiftoolStdOutWriter, [NotNull] ExifToolStdErrWriter exiftoolStdErrWriter)
        {
            var stdOutWriter = logger is NullLogger ? (IBytesWriter)new BytesWriterLogDecorator(exiftoolStdOutWriter, logger, "ExifTool stdout") : exiftoolStdOutWriter;
            var stdErrWriter = logger is NullLogger ? (IBytesWriter)new BytesWriterLogDecorator(exiftoolStdErrWriter, logger, "ExifTool stderr") : exiftoolStdErrWriter;

            return CreateShell(
                               exifToolFullPath,
                               args,
                               new WriteDelegatedDummyStream(stdOutWriter),
                               new WriteDelegatedDummyStream(stdErrWriter));
        }

        internal virtual IShell CreateShell(string exifToolFullPath, IEnumerable<string> args, Stream outputStream, Stream errStream)
        {
            var medallionShellAdapter = new MedallionShellAdapter(exifToolFullPath, args, outputStream, errStream);

            if (logger is NullLogger)
                return medallionShellAdapter;

            return new LoggingShellDecorator(medallionShellAdapter, logger);
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

        private async Task DisposeAsyncImplementation()
        {
            var ct = CancellationToken.None;

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

                    try
                    {
                        using var cts = new CancellationTokenSource(timeout);
                        await ExecuteOnlyAsync(command, CancellationToken.None)
                            .WithWaitCancellation(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // ignore
                    }

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
                        if (logger.IsEnabled(LogLevel.Error))
                            logger.Log(new LogEntry(LogLevel.Error, "Killing the process threw an exception.", e));
                    }
                }

                stream.Update -= StreamOnUpdate;
                errorStream.Error -= StreamOnError;
                UnsubscribeCmdOnProcessExitedOnce();
                shell = null;
                disposed = true;
                disposing = false;
            }
        }

        private async Task<string> ExecuteImpAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                var tcs = new TaskCompletionSource<string>();

#if FEATURE_ASYNC_DISPOSABLE
                await using (ct.Register(() => tcs.TrySetCanceled()))
#else
                using (ct.Register(() => tcs.TrySetCanceled()))
#endif
                {
                    key++;
                    var keyString = key.ToString();

                    if (!waitingTasks.TryAdd(keyString, tcs))
                        throw new Exception("Could not execute");

                    await AddToExifToolAsync(keyString, args).ConfigureAwait(false);
                    return await tcs.Task.ConfigureAwait(false);
                }
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

        private void StreamOnError(object sender, ErrorCapturedArgs e)
        {
            foreach (var item in waitingTasks.ToArray())
            {
                if (waitingTasks.TryRemove(item.Key, out var tcs))
                    tcs.TrySetException(new Exception(e.Data));
            }
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
