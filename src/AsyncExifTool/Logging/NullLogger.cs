namespace CoenM.ExifToolLib.Logging
{
    internal sealed class NullLogger : ILogger
    {
        public void Log(LogEntry entry)
        {
            // intentionally do nothing
        }

        public bool IsEnabled(LogLevel logLevel) => false;
    }
}
