namespace CoenM.ExifToolLib.Logging
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    internal static class LoggerExtensions
    {
        internal static void Trace([NotNull] this ILogger @this, string message)
        {
            if (!@this.IsEnabled(LogLevel.Trace))
                return;

            @this.Log(new LogEntry(LogLevel.Trace, message));
        }

        internal static void Debug([NotNull] this ILogger @this, string message)
        {
            if (!@this.IsEnabled(LogLevel.Debug))
                return;

            @this.Log(new LogEntry(LogLevel.Debug, message));
        }

        internal static void Debug([NotNull] this ILogger @this, Func<string> messageFunc)
        {
            if (!@this.IsEnabled(LogLevel.Debug))
                return;
            if (messageFunc == null)
                return;

            @this.Log(new LogEntry(LogLevel.Debug, messageFunc.Invoke()));
        }

        internal static void Info([NotNull] this ILogger @this, string message)
        {
            if (!@this.IsEnabled(LogLevel.Info))
                return;

            @this.Log(new LogEntry(LogLevel.Info, message));
        }

        internal static void Warn([NotNull] this ILogger @this, string message)
        {
            if (!@this.IsEnabled(LogLevel.Warn))
                return;

            @this.Log(new LogEntry(LogLevel.Warn, message));
        }

        internal static void Error([NotNull] this ILogger @this, string message)
        {
            if (!@this.IsEnabled(LogLevel.Error))
                return;

            @this.Log(new LogEntry(LogLevel.Error, message));
        }

        internal static void Fatal([NotNull] this ILogger @this, string message)
        {
            if (!@this.IsEnabled(LogLevel.Fatal))
                return;

            @this.Log(new LogEntry(LogLevel.Fatal, message));
        }
    }
}
