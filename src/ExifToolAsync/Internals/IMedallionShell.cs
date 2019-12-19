namespace ExifToolAsync.Internals
{
    using System;
    using System.Threading.Tasks;
    using Medallion.Shell;

    internal interface IMedallionShell
    {
        event EventHandler ProcessExited;

        Task<CommandResult> Task { get; }

        void Kill();

        Task WriteLineAsync(string text);
    }
}
