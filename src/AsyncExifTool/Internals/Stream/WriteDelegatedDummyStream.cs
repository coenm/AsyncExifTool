namespace CoenM.ExifToolLib.Internals.Stream
{
    using System.IO;
    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;

    internal class WriteDelegatedDummyStream : Stream
    {
        private readonly IBytesWriter _delegate;

        public WriteDelegatedDummyStream([NotNull] IBytesWriter @delegate)
        {
            Guard.NotNull(@delegate, nameof(@delegate));
            _delegate = @delegate;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => default;

        public override long Position
        {
            get => default;

            set
            {
                // intentionally do nothing.
            }
        }

        public override void Flush()
        {
            // intentionally do nothing.
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // intentionally do nothing.
            return default;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // intentionally do nothing.
            return default;
        }

        public override void SetLength(long value)
        {
            // intentionally do nothing.
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _delegate.Write(buffer, offset, count);
        }
    }
}
