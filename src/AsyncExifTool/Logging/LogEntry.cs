namespace CoenM.ExifToolLib.Logging
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Immutable LogEntry that contains the log information.
    /// </summary>
    // https://stackoverflow.com/questions/5646820/logger-wrapper-best-practice/5646876#5646876
    public readonly struct LogEntry
    {
        internal LogEntry(LogLevel severity, [NotNull] string message, Exception exception = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            Severity = severity;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Severity of the log entry.
        /// </summary>
        public LogLevel Severity { get; }

        /// <summary>
        /// Log message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Exception. Can be null.
        /// </summary>
        public Exception Exception { get; }
    }
}
