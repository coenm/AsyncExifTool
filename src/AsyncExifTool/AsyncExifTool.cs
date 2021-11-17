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
        private readonly string _exifToolPath;
        private readonly AsyncLock _executeAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock _executeImpAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock _disposingSyncLock = new AsyncLock();
        private readonly object _cmdExitedSubscribedSyncLock = new object();
        private readonly object _initializedSyncLock = new object();
        private readonly CancellationTokenSource _stopQueueCts;

        private readonly string[] _exifToolArguments;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _waitingTasks;
        private readonly ExifToolStdOutWriter _stream;
        private readonly ExifToolStdErrWriter _errorStream;
        private readonly AsyncManualResetEvent _cmdExitedMre;
        private readonly ILogger _logger;
        private IShell _shell;
        private int _key;
        private bool _disposed;
        private bool _disposing;
        private bool _cmdExited;
        private bool _cmdExitedSubscribed;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncExifTool"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for Exiftool.</param>
        public AsyncExifTool([NotNull] AsyncExifToolConfiguration configuration)
            : this(configuration, new NullLogger())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncExifTool"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for Exiftool.</param>
        /// <param name="logger">The logger.</param>
        public AsyncExifTool([NotNull] AsyncExifToolConfiguration configuration, [NotNull] ILogger logger)
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(logger, nameof(logger));

            this._logger = logger;
            _stream = new ExifToolStdOutWriter(configuration.ExifToolEncoding);
            _errorStream = new ExifToolStdErrWriter(configuration.ExifToolEncoding);
            _stopQueueCts = new CancellationTokenSource();
            _initialized = false;
            _disposed = false;
            _disposing = false;
            _cmdExited = false;
            _cmdExitedMre = new AsyncManualResetEvent(false);
            _key = 0;
            _exifToolPath = configuration.ExifToolFullFilename;

            _exifToolArguments = Enumerable.Empty<string>()
                .Concat(ExifToolArguments.ExifToolArgumentsConfigFile(configuration.ConfigurationFilename))
                .Concat(ExifToolArguments.StayOpenMode(true))
                .Concat(ExifToolArguments.ReadCommandLineArgumentsFromStdIn())
                .Concat(configuration.CommonArgs)
                .ToArray();

            _waitingTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        }

        /// <summary>
        /// Initialize <see cref="AsyncExifTool"/>. This will start the ExifTool process on the host.
        /// </summary>
        /// <exception cref="AsyncExifToolInitialisationException">Thrown when the ExifTool process throws an exception when started.</exception>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            lock (_initializedSyncLock)
            {
                if (_initialized)
                {
                    return;
                }

                _logger.Info("Initializing");

                _stream.Update += StreamOnUpdate;
                _errorStream.Error += StreamOnError;

                _shell = CreateShell(_exifToolPath, _exifToolArguments, _stream, _errorStream);
                _shell.ProcessExited += ShellOnProcessExited;
                _cmdExitedSubscribed = true;

                try
                {
                    _shell.Initialize();
                }
                catch (Exception e)
                {
                    _shell.ProcessExited -= ShellOnProcessExited;
                    _cmdExitedSubscribed = false;

                    throw new AsyncExifToolInitialisationException("Could not initialise exiftool", e);
                }

                _initialized = true;

                _logger.Info("Initialized");
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
            if (!_initialized)
            {
                throw new Exception("Not initialized");
            }

            if (_disposed)
            {
                throw new Exception("Disposed");
            }

            if (_disposing)
            {
                throw new Exception("Disposing");
            }

            _logger.Debug($"{nameof(ExecuteAsync)} - Wait before entering {nameof(_executeAsyncSyncLock)}.");
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _stopQueueCts.Token);
            using (await _executeAsyncSyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
            {
                _logger.Debug($"{nameof(ExecuteAsync)} - Entered {nameof(_executeAsyncSyncLock)}.");
                var result = await ExecuteImpAsync(args, ct).ConfigureAwait(false);
                _logger.Debug($"{nameof(ExecuteAsync)} - Released {nameof(_executeAsyncSyncLock)}.");
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
            IBytesWriter stdOutWriter = _logger is NullLogger ? (IBytesWriter)new BytesWriterLogDecorator(exiftoolStdOutWriter, _logger, "ExifTool stdout") : exiftoolStdOutWriter;
            IBytesWriter stdErrWriter = _logger is NullLogger ? (IBytesWriter)new BytesWriterLogDecorator(exiftoolStdErrWriter, _logger, "ExifTool stderr") : exiftoolStdErrWriter;

            return CreateShell(
                               exifToolFullPath,
                               args,
                               new WriteDelegatedDummyStream(stdOutWriter),
                               new WriteDelegatedDummyStream(stdErrWriter));
        }

        internal virtual IShell CreateShell(string exifToolFullPath, IEnumerable<string> args, Stream outputStream, Stream errStream)
        {
            var medallionShellAdapter = new MedallionShellAdapter(exifToolFullPath, args, outputStream, errStream);

            if (_logger is NullLogger)
            {
                return medallionShellAdapter;
            }

            return new LoggingShellDecorator(medallionShellAdapter, _logger);
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
            if (!_initialized)
            {
                return;
            }

            if (_disposed)
            {
                return;
            }

            using (await _disposingSyncLock.LockAsync(CancellationToken.None).ConfigureAwait(false))
            {
                if (_disposed)
                {
                    return;
                }

                _disposing = true;

                // cancel all requests to process.
                _stopQueueCts?.Cancel();

                var timeout = TimeSpan.FromMilliseconds(100);
                if (!_cmdExited)
                {
                    // This is really not okay. Not sure why or when the stay-open False command doesn't seem to work.
                    // This is just a stupid 'workaround' and is okay for now.
                    await _cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                }

                if (!_cmdExited)
                {
                    // Try quit ExifTool process using '-stay_open' 'false' arguments.
                    var command = new[] { ExifToolArguments.STAY_OPEN, ExifToolArguments.BOOL_FALSE };

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

                    await _cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                }

                if (!_cmdExited)
                {
                    // Try quit ExifTool process by sending Ctrl-C to the process.
                    // This does not always work (depending on the OS, and if the process runs in a console or not).
                    await _shell.TryCancelAsync().ConfigureAwait(false);
                    await _cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                }

                if (!_cmdExited)
                {
                    // Try kill the process.
                    try
                    {
                        _shell.Kill();
                        await _cmdExitedMre.WaitOneAsync(timeout).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.Log(new LogEntry(LogLevel.Error, "Killing the process threw an exception.", e));
                        }
                    }
                }

                _stream.Update -= StreamOnUpdate;
                _errorStream.Error -= StreamOnError;
                UnsubscribeCmdOnProcessExitedOnce();
                _shell = null;
                _disposed = true;
                _disposing = false;
            }
        }

        private async Task<string> ExecuteImpAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await _executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                var tcs = new TaskCompletionSource<string>();

#if FEATURE_ASYNC_DISPOSABLE
                await using (ct.Register(() => tcs.TrySetCanceled()))
#else
                using (ct.Register(() => tcs.TrySetCanceled()))
#endif
                {
                    _key++;
                    var keyString = _key.ToString();

                    if (!_waitingTasks.TryAdd(keyString, tcs))
                    {
                        throw new Exception("Could not execute");
                    }

                    await AddToExifToolAsync(keyString, args).ConfigureAwait(false);
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
        }

        private async Task ExecuteOnlyAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await _executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                await AddToExifToolAsync(null, args).ConfigureAwait(false);
            }
        }

        private async Task AddToExifToolAsync(string executeKey, [NotNull] IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                await _shell.WriteLineAsync(arg).ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(executeKey))
            {
                await _shell.WriteLineAsync($"-execute{executeKey}").ConfigureAwait(false);
            }
        }

        private void StreamOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            if (!_waitingTasks.TryRemove(dataCapturedArgs.Key, out TaskCompletionSource<string> tcs))
            {
                return;
            }

            tcs.TrySetResult(dataCapturedArgs.Data);
        }

        private void StreamOnError(object sender, ErrorCapturedArgs e)
        {
            foreach (KeyValuePair<string, TaskCompletionSource<string>> item in _waitingTasks.ToArray())
            {
                if (_waitingTasks.TryRemove(item.Key, out TaskCompletionSource<string> tcs))
                {
                    tcs.TrySetException(new Exception(e.Data));
                }
            }
        }

        private void ShellOnProcessExited(object sender, EventArgs eventArgs)
        {
            _cmdExited = true;
            _cmdExitedMre.Set();
            UnsubscribeCmdOnProcessExitedOnce();
        }

        private void UnsubscribeCmdOnProcessExitedOnce()
        {
            if (!_cmdExitedSubscribed)
            {
                return;
            }

            lock (_cmdExitedSubscribedSyncLock)
            {
                if (!_cmdExitedSubscribed)
                {
                    return;
                }

                _cmdExitedSubscribed = false;
                _shell.ProcessExited -= ShellOnProcessExited;
            }
        }
    }
}
