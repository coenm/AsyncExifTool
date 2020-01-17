namespace CoenM.ExifToolLib.Internals.Stream
{
    using System;
    using System.IO;
    using System.Text;

    using CoenM.ExifToolLib.Internals.Guards;
    using CoenM.ExifToolLib.Logging;
    using JetBrains.Annotations;

    internal class ExifToolErrorStream : Stream
    {
        private readonly ILogger logger;
        private readonly Encoding encoding;

        public ExifToolErrorStream(
            [NotNull] ILogger logger,
            [CanBeNull] Encoding encoding)
        {
            Guard.NotNull(logger, nameof(logger));
            this.logger = logger;
            this.encoding = encoding ?? new UTF8Encoding();
        }

        public event EventHandler<ErrorCapturedArgs> Error;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position
        {
            get => 0;
            set
            {
                // intentionally do nothing.
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
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
            logger?.Warn("ExifToolErrorStream: " + stringData);
            Error?.Invoke(this, new ErrorCapturedArgs(stringData));
        }

        public override void Flush()
        {
            // intentionally do nothing.
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
            // intentionally do nothing.
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Write only stream.");
        }
    }
}
