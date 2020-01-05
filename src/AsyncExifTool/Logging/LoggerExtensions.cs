namespace CoenM.ExifToolLib.Logging
{
    using System;

    internal static class LoggerExtensions
    {
        internal static void Debug(this ILogger @this, string message)
        {
            if (@this == null)
                return;
            if (!@this.IsEnabled(LogLevel.Debug))
                return;

            @this.Log(new LogEntry(LogLevel.Debug, message));
        }

        internal static void Debug(this ILogger @this, Func<string> messageFunc)
        {
            if (@this == null)
                return;
            if (!@this.IsEnabled(LogLevel.Debug))
                return;
            if (messageFunc == null)
                return;

            @this.Log(new LogEntry(LogLevel.Debug, messageFunc.Invoke()));
        }

        internal static void Info(this ILogger @this, string message)
        {
            if (@this == null)
                return;
            if (!@this.IsEnabled(LogLevel.Info))
                return;

            @this.Log(new LogEntry(LogLevel.Info, message));
        }

        internal static void Warn(this ILogger @this, string message)
        {
            if (@this == null)
                return;
            if (!@this.IsEnabled(LogLevel.Warn))
                return;

            @this.Log(new LogEntry(LogLevel.Warn, message));
        }

        internal static void Error(this ILogger @this, string message)
        {
            if (@this == null)
                return;
            if (!@this.IsEnabled(LogLevel.Error))
                return;

            @this.Log(new LogEntry(LogLevel.Error, message));
        }

        internal static void Fatal(this ILogger @this, string message)
        {
            if (@this == null)
                return;
            if (!@this.IsEnabled(LogLevel.Fatal))
                return;

            @this.Log(new LogEntry(LogLevel.Fatal, message));
        }
    }
}
