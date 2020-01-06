namespace CoenM.ExifToolLib.Logging
{
    using JetBrains.Annotations;

    public interface ILogger
    {
        /// <summary>
        /// Writes the given LogEntry to log.
        /// </summary>
        /// <param name="entry">log entry to write to log.</param>
        void Log([NotNull] LogEntry entry);

        /// <summary>
        /// Verifies if logging for given <see cref="LogLevel"/> is enabled.
        /// </summary>
        /// <param name="logLevel">LogLevel.</param>
        /// <returns><c>true</c> if logging for level <paramref name="logLevel"/> is enabled, <c>false</c> otherwise.</returns>
        bool IsEnabled(LogLevel logLevel);
    }
}
