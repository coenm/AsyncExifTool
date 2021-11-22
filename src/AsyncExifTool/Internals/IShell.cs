namespace CoenM.ExifToolLib.Internals
{
    using System;
    using System.Threading.Tasks;

    internal interface IShell
    {
        event EventHandler? ProcessExited;

        Task<IShellResult> Task { get; }

        void Initialize();

        Task WriteLineAsync(string text);

        void Kill();

        Task<bool> TryCancelAsync();
    }
}
