namespace CoenM.ExifToolLib.Internals
{
    internal interface IShellResult
    {
        /// <summary>
        /// The exit code of the command's process
        /// </summary>
        int ExitCode { get; }

        /// <summary>
        /// Returns true iff the exit code is 0 (indicating success)
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// If available, the full standard output text of the command
        /// </summary>
        string StandardOutput { get; }

        /// <summary>
        /// If available, the full standard error text of the command
        /// </summary>
        string StandardError { get; }
    }
}
