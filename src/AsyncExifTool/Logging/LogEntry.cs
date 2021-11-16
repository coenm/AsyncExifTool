namespace CoenM.ExifToolLib.Logging
{
    using System;
    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;

    /// <summary>
    /// Immutable LogEntry that contains the log information.
    /// </summary>
    // https://stackoverflow.com/questions/5646820/logger-wrapper-best-practice/5646876#5646876
    public readonly struct LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> struct.
        /// </summary>
        /// <param name="severity">Log severity.</param>
        /// <param name="message">Log message. Cannot be null or empty.</param>
        /// <param name="exception">Exception to log. Optional. Can be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is empty or whitespace.</exception>
        internal LogEntry(LogLevel severity, [NotNull] string message, Exception exception = null)
        {
            Guard.NotNullOrWhiteSpace(message, nameof(message));
            Severity = severity;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Gets the severity of the log entry.
        /// </summary>
        public LogLevel Severity { get; }

        /// <summary>
        /// Gets the log message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception. Can be null.
        /// </summary>
        public Exception Exception { get; }
    }
}
