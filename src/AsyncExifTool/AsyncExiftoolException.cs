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
        private const int CurrentSerializationVersion = 1;
        private const string VersionKey = "v";
        private const string ExitCodeKey = "exit";
        private const string StandardOutputKey = "stdout";
        private const string StandardErrorKey = "stderr";

        internal AsyncExifToolException(int exitCode, string standardOutput, string standardError)
            : base(standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        // Required because AsyncExiftoolException implements ISerializable interface.
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private AsyncExifToolException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var version = info.GetInt32(VersionKey);

            switch (version)
            {
                // make sure we are backward compatible.
                case 1:
                    ExitCode = info.GetInt32(ExitCodeKey);
                    StandardOutput = info.GetString(StandardOutputKey);
                    StandardError = info.GetString(StandardErrorKey);
                    break;

                default:
                    throw new SerializationException($"Not capable of deserializing version {version}. Please update your version of {nameof(AsyncExifToolException)} or your data was corrupt.");
            }
        }

        /// <summary>
        /// ExitCode of the ExifTool process.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Standard output of the ExifTool process.
        /// </summary>
        public string StandardOutput { get; }

        /// <summary>
        /// Standard error of the ExifTool process.
        /// </summary>
        public string StandardError { get; }

        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            // if you need to change this implementation, you probably need to upgrade to the next version.
            // to be backwards compatible. Also you need to update the constructor.
            info.AddValue(VersionKey, CurrentSerializationVersion);

            info.AddValue(ExitCodeKey, ExitCode);
            info.AddValue(StandardOutputKey, StandardOutput);
            info.AddValue(StandardErrorKey, StandardError);

            base.GetObjectData(info, context);
        }
    }
}
