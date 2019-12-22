namespace ExifToolAsync.Internals
{
    using System;
    using System.Threading.Tasks;

    internal interface IShell
    {
        event EventHandler ProcessExited;

        Task<IShellResult> Task { get; }

        void Kill();

        Task WriteLineAsync(string text);
        Task CancelAsync();
    }
}
