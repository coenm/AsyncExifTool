namespace CoenM.ExifToolLib
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class AsyncExiftoolException : Exception
    {
        private const int CurrentSerializationVersion = 1;
        private const string VersionKey = "v";
        private const string ExitCodeKey = "exit";
        private const string StandardOutputKey = "stdout";
        private const string StandardErrorKey = "stderr";

        public AsyncExiftoolException(int exitCode, string standardOutput, string standardError)
            : base(standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private AsyncExiftoolException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var version = info.GetInt32(VersionKey);

            if (version != CurrentSerializationVersion)
                throw new SerializationException("Could not deserialize data");

            ExitCode = info.GetInt32(ExitCodeKey);
            StandardOutput = info.GetString(StandardOutputKey);
            StandardError = info.GetString(StandardErrorKey);
        }

        public int ExitCode { get; private set; }

        public string StandardOutput { get; private set; }

        public string StandardError { get; private set; }


        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(VersionKey, CurrentSerializationVersion);
            info.AddValue(ExitCodeKey, ExitCode);
            info.AddValue(StandardOutputKey, StandardOutput);
            info.AddValue(StandardErrorKey, StandardError);

            base.GetObjectData(info, context);
        }
    }
}
