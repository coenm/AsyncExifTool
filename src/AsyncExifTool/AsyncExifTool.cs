namespace CoenM.ExifToolLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals;
    using CoenM.ExifToolLib.Internals.AsyncManualResetEvent;
    using CoenM.ExifToolLib.Internals.MedallionShell;
    using CoenM.ExifToolLib.Internals.Stream;
    using JetBrains.Annotations;
    using Nito.AsyncEx;

    public class AsyncExifTool : IAsyncDisposable
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
        private IShell shell;
        private int key;
        private bool disposed;
        private bool disposing;
        private bool cmdExited;
        private bool cmdExitedSubscribed;
        private bool initialized;

        public AsyncExifTool([NotNull] AsyncExifToolConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

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

        public AsyncExifTool(string exifToolPath)
            : this (new AsyncExifToolConfiguration(
                exifToolPath,
                Encoding.UTF8,
                ExifToolExecutable.NewLine, new List<string>
                {
                    ExifToolArguments.CommonArgs,
                }))
        {
        }

        public void Initialize()
        {
            if (initialized)
                return;

            lock (initializedSyncLock)
            {
                if (initialized)
                    return;

                stream.Update += StreamOnUpdate;

                shell = CreateShell(exifToolPath, exifToolArguments, stream, null);

                // possible race condition.. to fix
                shell.ProcessExited += ShellOnProcessExited;

                cmdExitedSubscribed = true;
                initialized = true;
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

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, stopQueueCts.Token);
            using (await executeAsyncSyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
            {
                return await ExecuteImpAsync(args, ct).ConfigureAwait(false);
            }
        }

        public async Task DisposeAsync(CancellationToken ct)
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
                    var command = new[] {ExifToolArguments.StayOpen, ExifToolArguments.BoolFalse};
//                    try
//                    {
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);;
//                    }
//                    catch (Exception)
//                    {
//                        // ignore
//                    }

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

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(CancellationToken.None).ConfigureAwait(false);
//             using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
//             await DisposeAsync(cts.Token).ConfigureAwait(false);
        }

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

                await using (ct.Register(() => tcs.TrySetCanceled()))
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

        private async Task AddToExifToolAsync(string key, [NotNull] IEnumerable<string> args)
        {
            foreach (var arg in args)
                await shell.WriteLineAsync(arg).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(key))
                await shell.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
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
