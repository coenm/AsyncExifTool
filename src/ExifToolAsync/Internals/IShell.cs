namespace ExifToolAsync.Internals
{
    using System;
    using System.Threading.Tasks;

    internal interface IShell
    {
        event EventHandler ProcessExited;

        Task<IShellResult> Task { get; }
        
        Task WriteLineAsync(string text);

        void Kill();

        Task CancelAsync();
    }
}
