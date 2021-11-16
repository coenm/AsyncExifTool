namespace CoenM.ExifToolLib.Internals
{
    internal interface IShellResult
    {
        /// <summary>
        /// Gets the exit code of the command's process.
        /// </summary>
        int ExitCode { get; }

        /// <summary>
        /// Gets a value indicating whether the the process executed successfully (<c>true</c> when the <seealso cref="ExitCode"/> is <c>0</c>, and <c>false</c> otherwise).
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Gets the full standard output text of the command when available.
        /// </summary>
        string StandardOutput { get; }

        /// <summary>
        /// Gets the full standard error text of the command when available.
        /// </summary>
        string StandardError { get; }
    }
}
