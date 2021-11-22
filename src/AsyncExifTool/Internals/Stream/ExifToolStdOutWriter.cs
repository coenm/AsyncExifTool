namespace CoenM.ExifToolLib.Internals.Stream
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;

    internal class ExifToolStdOutWriter : IBytesWriter
    {
        private const int ONE_MB = 1024 * 1024;
        private readonly Encoding _encoding;
        private readonly byte[] _cache;
        private readonly byte[] _endOfMessageSequenceStart;
        private readonly byte[] _endOfMessageSequenceEnd;
        private readonly int _bufferSize;
        private int _index;

        public ExifToolStdOutWriter(
            [NotNull] Encoding encoding,
            int bufferSize = ONE_MB)
        {
            Guard.NotNull(encoding, nameof(encoding));
            Guard.MustBeGreaterThan(bufferSize, 0, nameof(bufferSize));

            var prefix = "{ready";
            var suffix = "}" + Environment.NewLine;

            _bufferSize = bufferSize;
            _encoding = encoding;
            _cache = new byte[_bufferSize];
            _index = 0;
            _endOfMessageSequenceStart = _encoding.GetBytes(prefix);
            _endOfMessageSequenceEnd = _encoding.GetBytes(suffix);
        }

        public event EventHandler<DataCapturedArgs> Update;

        public void Reset()
        {
            _index = 0;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (buffer == null)
            {
                return;
            }

            if (count <= 0)
            {
                return;
            }

            if (offset + count > buffer.Length)
            {
                return;
            }

            if (count > _bufferSize - _index)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            Array.Copy(buffer, 0, _cache, _index, count);
            _index += count;

            var lastEndIndex = 0;

            for (var i = 0; i < _index - 1; i++)
            {
                var j = 0;
                while (j < _endOfMessageSequenceStart.Length && _cache[i + j] == _endOfMessageSequenceStart[j])
                {
                    j++;
                }

                if (j != _endOfMessageSequenceStart.Length)
                {
                    continue;
                }

                j += i;

                // expect numbers as key.
                var keyStartIndex = j;
                while (j < _index && _cache[j] >= '0' && _cache[j] <= '9')
                {
                    j++;
                }

                if (keyStartIndex == j)
                {
                    continue;
                }

                var keyLength = j - keyStartIndex;

                var k = 0;
                while (k < _endOfMessageSequenceEnd.Length && _cache[j + k] == _endOfMessageSequenceEnd[k])
                {
                    k++;
                }

                if (k != _endOfMessageSequenceEnd.Length)
                {
                    continue;
                }

                j += k;

                var content = _encoding.GetString(_cache, lastEndIndex, i - lastEndIndex);
                var key = _encoding.GetString(_cache, keyStartIndex, keyLength);
                Update?.Invoke(this, new DataCapturedArgs(key, content));

                i = j;
                lastEndIndex = j;
            }

            Debug.Assert(lastEndIndex <= _index, "Expect that lastEndindex is less then index");

            if (lastEndIndex == 0)
            {
                return;
            }

            if (_index > lastEndIndex)
            {
                Array.Copy(_cache, lastEndIndex, _cache, 0, _index - lastEndIndex);
            }

            _index -= lastEndIndex;
        }
    }
}
