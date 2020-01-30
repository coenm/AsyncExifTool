namespace CoenM.ExifToolLib.Internals.Stream
{
    using System;
    using System.Text;

    using CoenM.ExifToolLib.Internals.Guards;

    using JetBrains.Annotations;

    internal class ExifToolStdErrWriter : IBytesWriter
    {
        [NotNull] private readonly string endLine;
        private readonly Encoding encoding;

        public ExifToolStdErrWriter([CanBeNull] Encoding encoding, [NotNull] string endLine)
        {
            Guard.NotNullOrEmpty(endLine, nameof(endLine));

            this.encoding = encoding ?? new UTF8Encoding();
            this.endLine = endLine;
        }

        public event EventHandler<ErrorCapturedArgs> Error;

        public void Reset()
        {
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (buffer == null)
                return;
            if (count == 0)
                return;
            if (offset + count > buffer.Length)
                return;

            var stringData = encoding.GetString(buffer, offset, count);
            Error?.Invoke(this, new ErrorCapturedArgs(stringData));
        }
    }
}
