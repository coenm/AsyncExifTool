namespace ExifToolAsync.ExifTool
{
    using System;
    using System.Threading.Tasks;
    using Medallion.Shell;

    public interface IMedallionShell
    {
        event EventHandler ProcessExited;

        Task<CommandResult> Task { get; }

        void Kill();

        Task WriteLineAsync(string text);
    }
}
