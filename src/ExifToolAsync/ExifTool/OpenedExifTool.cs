namespace EagleEye.ExifTool.ExifTool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;
    using Nito.AsyncEx;

    public class OpenedExifTool : IExifTool
    {
        private readonly string exifToolPath;
        private readonly AsyncLock executeAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock executeImpAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock disposingSyncLock = new AsyncLock();
        private readonly object cmdExitedSubscribedSyncLock = new object();
        private readonly object initializedSyncLock = new object();
        private readonly CancellationTokenSource stopQueueCts;

        private readonly List<string> defaultArgs;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> waitingTasks;
        private readonly ExifToolStayOpenStream stream;
        private IMedallionShell cmd;
        private int key;
        private bool disposed;
        private bool disposing;
        private bool cmdExited;
        private bool cmdExitedSubscribed;
        private bool initialized;

        public OpenedExifTool(string exifToolPath)
        {
            stream = new ExifToolStayOpenStream(Encoding.UTF8);

            stopQueueCts = new CancellationTokenSource();
            initialized = false;
            disposed = false;
            disposing = false;
            cmdExited = false;
            key = 0;
            this.exifToolPath = exifToolPath;
            defaultArgs = new List<string>
            {
                ExifToolArguments.StayOpen,
                ExifToolArguments.BoolTrue,
                "-@",
                "-",
                ExifToolArguments.CommonArgs,
                ExifToolArguments.JsonOutput,

                // format coordinates as signed decimals.
                "-c",
                "%+.6f",

                "-struct",
                "-g", // group
            };

            waitingTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        }

        public void Init()
        {
            if (initialized)
                return;

            lock (initializedSyncLock)
            {
                if (initialized)
                    return;

                stream.Update += StreamOnUpdate;

                cmd = CreateExitToolMedallionShell(exifToolPath, defaultArgs, stream, null);

                cmd.ProcessExited += CmdOnProcessExited;
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

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, stopQueueCts.Token);

            using (await executeAsyncSyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
            {
                return await ExecuteImpAsync(args, ct).ConfigureAwait(false);
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
                Ignore(() => stopQueueCts?.Cancel());

                try
                {
                    if (!cmdExited)
                    {
                        // This is really not okay. Not sure why or when the stay-open False command doesn't seem to work.
                        // This is just a stupid 'workaround' and is okay for now.
                        await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);

                        var command = new[] { ExifToolArguments.StayOpen, ExifToolArguments.BoolFalse };
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);

                        if (!cmdExited)
                            await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);

                        var retry = 0;
                        while (retry < 3 && cmdExited == false)
                        {
                            try
                            {
                                await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                                if (!cmdExited)
                                    await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
                            }
                            catch (Exception)
                            {
                                // ignore
                            }
                            finally
                            {
                                retry++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred when executing stay_open false. Msg: {e.Message}");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() => cmd?.Kill(), CancellationToken.None);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    stream.Update -= StreamOnUpdate;

                    Ignore(() => stream.Dispose());
                    cmd = null;

                    return;
                }

                // else try to dispose gracefully
                if (cmdExited == false && cmd?.Task != null)
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        cmd.Kill();

                        // why?
                        await cmd.Task.ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        sw.Stop();
                        Console.WriteLine($"Exception occurred after {sw.Elapsed} when awaiting ExifTool task. Msg: {e.Message}");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(() => cmd?.Kill(), CancellationToken.None);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }

                stream.Update -= StreamOnUpdate;
                Ignore(UnsubscribeCmdOnProcessExitedOnce);
                Ignore(() => stream.Dispose());
                cmd = null;
                disposed = true;
                disposing = false;
            }
        }

        protected virtual IMedallionShell CreateExitToolMedallionShell(
            string exifToolPath,
            List<string> defaultArgs,
            Stream outputStream,
            Stream errorStream)
        {
            return new MedallionShellAdapter(exifToolPath, defaultArgs, outputStream, errorStream);
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
                using (ct.Register(() => tcs.TrySetCanceled()))
                {
                    this.key++;
                    var key = this.key.ToString();

                    if (!waitingTasks.TryAdd(key, tcs))
                        throw new Exception("Could not execute");

                    await AddToExifToolAsync(key, args).ConfigureAwait(false);
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
                await cmd.WriteLineAsync(arg).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(key))
                await cmd.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
        }

        private void StreamOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            if (waitingTasks.TryRemove(dataCapturedArgs.Key, out var tcs))
            {
                tcs.TrySetResult(dataCapturedArgs.Data);
            }
        }

        private void CmdOnProcessExited(object sender, EventArgs eventArgs)
        {
            cmdExited = true;
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
                Ignore(() => cmd.ProcessExited -= CmdOnProcessExited);
                cmdExitedSubscribed = false;
            }
        }
    }
}
