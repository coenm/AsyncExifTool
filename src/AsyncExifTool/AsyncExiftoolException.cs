namespace CoenM.ExifToolLib
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// AsyncExifTool Exception.
    /// </summary>
    [Serializable]
    public sealed class AsyncExifToolException : Exception
    {
        private const int CURRENT_SERIALIZATION_VERSION = 1;
        private const string VERSION_KEY = "v";
        private const string EXIT_CODE_KEY = "exit";
        private const string STANDARD_OUTPUT_KEY = "stdout";
        private const string STANDARD_ERROR_KEY = "stderr";

        internal AsyncExifToolException(int exitCode, string standardOutput, string standardError)
            : base(standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        // Required because AsyncExiftoolException implements ISerializable interface.
        private AsyncExifToolException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var version = info.GetInt32(VERSION_KEY);

            ExitCode = default;

            switch (version)
            {
                // make sure we are backward compatible.
                case 1:
                    ExitCode = info.GetInt32(EXIT_CODE_KEY);
                    StandardOutput = info.GetString(STANDARD_OUTPUT_KEY) ?? string.Empty;
                    StandardError = info.GetString(STANDARD_ERROR_KEY) ?? string.Empty;
                    break;

                default:
                    throw new SerializationException($"Not capable of deserializing version {version}. Please update your version of {nameof(AsyncExifToolException)} or your data was corrupt.");
            }
        }

        /// <summary>
        /// Gets the exit code of the ExifTool process.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Gets the standard output of the ExifTool process.
        /// </summary>
        public string StandardOutput { get; }

        /// <summary>
        /// Gets the standard error of the ExifTool process.
        /// </summary>
        public string StandardError { get; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            // if you need to change this implementation, you probably need to upgrade to the next version.
            // to be backwards compatible. Also you need to update the constructor.
            info.AddValue(VERSION_KEY, CURRENT_SERIALIZATION_VERSION);

            info.AddValue(EXIT_CODE_KEY, ExitCode);
            info.AddValue(STANDARD_OUTPUT_KEY, StandardOutput);
            info.AddValue(STANDARD_ERROR_KEY, StandardError);

            base.GetObjectData(info, context);
        }
    }
}
