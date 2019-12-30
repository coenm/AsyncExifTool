namespace CoenM.ExifToolLib.Internals.Stream
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using JetBrains.Annotations;

    internal class ExifToolStayOpenStream : Stream
    {
        private const int OneMb = 1024 * 1024;
        private readonly Encoding encoding;
        private readonly byte[] cache;
        private readonly byte[] endOfMessageSequenceStart;
        private readonly byte[] endOfMessageSequenceEnd;
        private readonly int bufferSize;
        private int index;

        public ExifToolStayOpenStream(
            [CanBeNull] Encoding encoding, 
            [NotNull] string endLine,
            int bufferSize = OneMb)
        {
            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (string.IsNullOrEmpty(endLine))
                throw new ArgumentNullException(nameof(endLine));

            var prefix = endLine + "{ready";
            var suffix = "}" + endLine;

            this.bufferSize = bufferSize;
            this.encoding = encoding ?? new UTF8Encoding();
            cache = new byte[this.bufferSize];
            index = 0;
            endOfMessageSequenceStart = this.encoding.GetBytes(prefix);
            endOfMessageSequenceEnd = this.encoding.GetBytes(suffix);
        }

        public event EventHandler<DataCapturedArgs> Update;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => index;

        public override long Position
        {
            get => index;
            set
            {
                // do nothing but also don't throw an exception.
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
            if (count > bufferSize - index)
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");

            Array.Copy(buffer, 0, cache, index, count);
            index += count;

            var lastEndIndex = 0;

            for (var i = 0; i < index - 1; i++)
            {
                var j = 0;
                while (j < endOfMessageSequenceStart.Length && cache[i + j] == endOfMessageSequenceStart[j])
                    j++;

                if (j != endOfMessageSequenceStart.Length)
                    continue;

                j += i;

                // expect numbers as key.
                var keyStartIndex = j;
                while (j < index && cache[j] >= '0' && cache[j] <= '9')
                    j++;

                if (keyStartIndex == j)
                    continue;

                var keyLength = j - keyStartIndex;

                var k = 0;
                while (k < endOfMessageSequenceEnd.Length && cache[j + k] == endOfMessageSequenceEnd[k])
                    k++;

                if (k != endOfMessageSequenceEnd.Length)
                    continue;

                j += k;

                var content = encoding.GetString(cache, lastEndIndex, i - lastEndIndex);
                var key = encoding.GetString(cache, keyStartIndex, keyLength);
                Update?.Invoke(this, new DataCapturedArgs(key, content));

                i = j;
                lastEndIndex = j;
            }

            Debug.Assert(lastEndIndex <= index, "Expect that lastEndindex is less then index");

            if (lastEndIndex == 0)
                return;

            if (index > lastEndIndex)
                Array.Copy(cache, lastEndIndex, cache, 0, index - lastEndIndex);

            index -= lastEndIndex;
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
