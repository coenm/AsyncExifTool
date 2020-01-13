namespace Samples
{
    using System;
    using CoenM.ExifToolLib.Logging;

    using ILogger = CoenM.ExifToolLib.Logging.ILogger;
    using LogLevel = CoenM.ExifToolLib.Logging.LogLevel;

    // Adapter in order to log to NLog.
    // Make sure the IsEnabled(LogLevel logLevel) method is fast as it is polled before each log write action. 
    // You can use any other log framework you want, just write an adapter for it.
    public class AsyncExifToolToNLogAdapter : ILogger
    {
        private readonly NLog.ILogger logger;

        public AsyncExifToolToNLogAdapter(NLog.ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Log(LogEntry entry)
        {
            if (entry.Exception == null)
                logger.Log(Convert(entry.Severity), entry.Message);
            else
                logger.Log(Convert(entry.Severity), entry.Exception, entry.Message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logger.IsEnabled(Convert(logLevel));
        }

        private static NLog.LogLevel Convert(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => NLog.LogLevel.Trace,
                LogLevel.Debug => NLog.LogLevel.Debug,
                LogLevel.Info => NLog.LogLevel.Info,
                LogLevel.Warn => NLog.LogLevel.Warn,
                LogLevel.Error => NLog.LogLevel.Error,
                LogLevel.Fatal => NLog.LogLevel.Fatal,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }
    }
}
