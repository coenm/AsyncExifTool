namespace CoenM.ExifToolLib.Internals.MedallionShell
{
    using System;
    using System.Threading.Tasks;
    using CoenM.ExifToolLib.Internals.Guards;
    using CoenM.ExifToolLib.Logging;

    internal class LoggingShellDecorator : IShell, IDisposable
    {
        private readonly IShell _decoratee;
        private readonly ILogger _logger;

        public LoggingShellDecorator(IShell decoratee, ILogger logger)
        {
            Guard.NotNull(decoratee, nameof(decoratee));
            Guard.NotNull(logger, nameof(logger));
            this._decoratee = decoratee;
            this._logger = logger;
        }

        public event EventHandler ProcessExited
        {
            add
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.Log(new LogEntry(LogLevel.Trace, $"Added shells {nameof(ProcessExited)} event handler."));
                }

                _decoratee.ProcessExited += value;
            }

            remove
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.Log(new LogEntry(LogLevel.Trace, $"Removed shells {nameof(ProcessExited)} event handler."));
                }

                _decoratee.ProcessExited -= value;
            }
        }

        public Task<IShellResult> Task => _decoratee.Task;

        public void Initialize()
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.Log(new LogEntry(LogLevel.Trace, "Start initialising shell"));
            }

            _decoratee.Initialize();
        }

        public Task WriteLineAsync(string text)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.Log(new LogEntry(LogLevel.Trace, $"WriteLineAsync: {text}"));
            }

            return _decoratee.WriteLineAsync(text);
        }

        public void Kill()
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.Log(new LogEntry(LogLevel.Trace, $"Killing shell"));
            }

            _decoratee.Kill();
        }

        public async Task<bool> TryCancelAsync()
        {
            if (!_logger.IsEnabled(LogLevel.Trace))
            {
                return await _decoratee.TryCancelAsync().ConfigureAwait(false);
            }

            _logger.Log(new LogEntry(LogLevel.Trace, "TryCancel the current shell process"));
            var result = await _decoratee.TryCancelAsync().ConfigureAwait(false);
            _logger.Log(new LogEntry(LogLevel.Trace, $"TryCancel returned {result}."));
            return result;
        }

        public void Dispose()
        {
            if (!(_decoratee is IDisposable disposable))
            {
                return;
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.Log(new LogEntry(LogLevel.Trace, "Dispose shell."));
            }

            disposable.Dispose();
        }
    }
}
