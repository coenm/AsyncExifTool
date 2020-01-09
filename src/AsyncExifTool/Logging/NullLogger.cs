namespace CoenM.ExifToolLib.Logging
{
    internal sealed class NullLogger : ILogger
    {
        private NullLogger()
        {
            // intentionally do nothing
        }

        public static NullLogger Instance { get; } = new NullLogger();

        public void Log(LogEntry entry)
        {
            // intentionally do nothing
        }

        public bool IsEnabled(LogLevel logLevel) => false;
    }
}
