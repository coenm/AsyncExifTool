namespace ExifToolAsync.ExifTool
{
    using System;

    public class ExiftoolException : Exception
    {
        public ExiftoolException(int exitCode, string standardOutput, string standardError)
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
