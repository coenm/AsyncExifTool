namespace CoenM.ExifToolLib.Internals.Stream
{
    using System;
    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;

    internal class DataCapturedArgs : EventArgs
    {
        public DataCapturedArgs(string key, string data)
        {
            DebugGuard.NotNullOrWhiteSpace(key, nameof(key));
            DebugGuard.NotNull(data, nameof(data)); // can be empty

            Key = key;
            Data = data;
        }

        public string Key { get; }

        public string Data { get; }
    }
}
