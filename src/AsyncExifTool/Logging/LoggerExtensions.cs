namespace CoenM.ExifToolLib.Logging
{
    using System;
    using JetBrains.Annotations;

    internal static class LoggerExtensions
    {
        internal static void Trace(this ILogger @this, string message)
        {
            @this.Log(new LogEntry(LogLevel.Trace, message));
        }

        internal static void Trace(this ILogger @this, Func<string> messageGenerator)
        {
            if (messageGenerator == null)
            {
                return;
            }

            if (!@this.IsEnabled(LogLevel.Trace))
            {
                return;
            }

            @this.Log(new LogEntry(LogLevel.Trace, messageGenerator.Invoke()));
        }

        internal static void Debug(this ILogger @this, string message)
        {
            @this.Log(new LogEntry(LogLevel.Debug, message));
        }

        internal static void Debug(this ILogger @this, Func<string> messageGenerator)
        {
            if (messageGenerator == null)
            {
                return;
            }

            if (!@this.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            @this.Log(new LogEntry(LogLevel.Debug, messageGenerator.Invoke()));
        }

        internal static void Info(this ILogger @this, string message)
        {
            @this.Log(new LogEntry(LogLevel.Info, message));
        }

        internal static void Info(this ILogger @this, Func<string> messageGenerator)
        {
            if (messageGenerator == null)
            {
                return;
            }

            if (!@this.IsEnabled(LogLevel.Info))
            {
                return;
            }

            @this.Log(new LogEntry(LogLevel.Info, messageGenerator.Invoke()));
        }

        internal static void Warn(this ILogger @this, string message)
        {
            @this.Log(new LogEntry(LogLevel.Warn, message));
        }

        internal static void Warn(this ILogger @this, Func<string> messageGenerator)
        {
            if (messageGenerator == null)
            {
                return;
            }

            if (!@this.IsEnabled(LogLevel.Warn))
            {
                return;
            }

            @this.Log(new LogEntry(LogLevel.Warn, messageGenerator.Invoke()));
        }

        internal static void Error(this ILogger @this, string message, Exception? exception)
        {
            @this.Log(new LogEntry(LogLevel.Error, message, exception));
        }

        internal static void Error(this ILogger @this, Func<string> messageGenerator, Exception? exception)
        {
            if (messageGenerator == null)
            {
                return;
            }

            if (!@this.IsEnabled(LogLevel.Error))
            {
                return;
            }

            @this.Log(new LogEntry(LogLevel.Error, messageGenerator.Invoke(), exception));
        }

        internal static void Fatal(this ILogger @this, string message, Exception? exception)
        {
            @this.Log(new LogEntry(LogLevel.Fatal, message, exception));
        }

        internal static void Fatal(this ILogger @this, Func<string> messageGenerator, Exception? exception)
        {
            if (messageGenerator == null)
            {
                return;
            }

            if (!@this.IsEnabled(LogLevel.Fatal))
            {
                return;
            }

            @this.Log(new LogEntry(LogLevel.Fatal, messageGenerator.Invoke(), exception));
        }
    }
}
