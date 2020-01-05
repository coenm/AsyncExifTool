namespace CoenM.ExifToolLib.Logging
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    // https://stackoverflow.com/questions/5646820/logger-wrapper-best-practice/5646876#5646876
    // Immutable DTO that contains the log information.
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

        public LogLevel Severity { get; }

        public string Message { get; }

        public Exception Exception { get; }
    }
}
