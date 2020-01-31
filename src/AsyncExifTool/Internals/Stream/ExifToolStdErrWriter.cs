namespace CoenM.ExifToolLib.Internals.Stream
{
    using System;
    using System.Text;

    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;

    internal class ExifToolStdErrWriter : IBytesWriter
    {
        private readonly Encoding encoding;

        public ExifToolStdErrWriter([NotNull] Encoding encoding)
        {
            Guard.NotNull(encoding, nameof(encoding));
            this.encoding = encoding;
        }

        public event EventHandler<ErrorCapturedArgs> Error;

        public void Reset()
        {
            // todo?
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (buffer == null)
                return;
            if (count <= 0)
                return;
            if (offset + count > buffer.Length)
                return;

            var stringData = encoding.GetString(buffer, offset, count);
            Error?.Invoke(this, new ErrorCapturedArgs(stringData));
        }
    }
}
