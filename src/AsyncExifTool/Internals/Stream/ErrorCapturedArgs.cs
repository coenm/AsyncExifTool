namespace CoenM.ExifToolLib.Internals.Stream
{
    using System;
    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;

    internal class ErrorCapturedArgs : EventArgs
    {
        public ErrorCapturedArgs([NotNull] string data)
        {
            DebugGuard.NotNull(data, nameof(data)); // can be empty
            Data = data;
        }

        public string Data { get; }
    }
}
