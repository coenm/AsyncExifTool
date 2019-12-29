namespace CoenM.ExifToolLib
{
    using System;

    [Serializable]
    public class AsyncExiftoolException : Exception
    {
        public AsyncExiftoolException(int exitCode, string standardOutput, string standardError)
            : base(standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        public int ExitCode { get; }

        public string StandardOutput { get; }

        public string StandardError { get; }
    }
}
