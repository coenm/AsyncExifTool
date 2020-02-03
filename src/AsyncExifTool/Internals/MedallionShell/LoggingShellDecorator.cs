namespace CoenM.ExifToolLib.Internals.MedallionShell
{
    using System;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals.Guards;
    using CoenM.ExifToolLib.Logging;

    internal class LoggingShellDecorator : IShell, IDisposable
    {
        private readonly IShell decoratee;
        private readonly ILogger logger;

        public LoggingShellDecorator(IShell decoratee, ILogger logger)
        {
            Guard.NotNull(decoratee, nameof(decoratee));
            Guard.NotNull(logger, nameof(logger));
            this.decoratee = decoratee;
            this.logger = logger;
        }

        public event EventHandler ProcessExited
        {
            add
            {
                if (logger.IsEnabled(LogLevel.Trace))
                    logger.Log(new LogEntry(LogLevel.Trace, $"Added shells {nameof(ProcessExited)} event handler."));

                decoratee.ProcessExited += value;
            }

            remove
            {
                if (logger.IsEnabled(LogLevel.Trace))
                    logger.Log(new LogEntry(LogLevel.Trace, $"Removed shells {nameof(ProcessExited)} event handler."));

                decoratee.ProcessExited -= value;
            }
        }

        public Task<IShellResult> Task => decoratee.Task;

        public void Initialize()
        {
            if (logger.IsEnabled(LogLevel.Trace))
                logger.Log(new LogEntry(LogLevel.Trace, "Start initialising shell"));
            decoratee.Initialize();
        }

        public Task WriteLineAsync(string text)
        {
            if (logger.IsEnabled(LogLevel.Trace))
                logger.Log(new LogEntry(LogLevel.Trace, $"WriteLineAsync: {text}"));

            return decoratee.WriteLineAsync(text);
        }

        public void Kill()
        {
            if (logger.IsEnabled(LogLevel.Trace))
                logger.Log(new LogEntry(LogLevel.Trace, $"Killing shell"));

            decoratee.Kill();
        }

        public async Task<bool> TryCancelAsync()
        {
            if (!logger.IsEnabled(LogLevel.Trace))
                return await decoratee.TryCancelAsync().ConfigureAwait(false);

            logger.Log(new LogEntry(LogLevel.Trace, "TryCancel the current shell process"));
            var result = await decoratee.TryCancelAsync().ConfigureAwait(false);
            logger.Log(new LogEntry(LogLevel.Trace, $"TryCancel returned {result}."));
            return result;
        }

        public void Dispose()
        {
            if (!(decoratee is IDisposable disposable))
                return;

            if (logger.IsEnabled(LogLevel.Trace))
                logger.Log(new LogEntry(LogLevel.Trace, "Dispose shell."));

            disposable.Dispose();
        }
    }
}
