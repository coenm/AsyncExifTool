namespace CoenM.ExifToolLib.Internals.Stream
{
    using System.Text;
    using CoenM.ExifToolLib.Internals.Guards;
    using CoenM.ExifToolLib.Logging;
    using JetBrains.Annotations;

    /// <summary>
    /// Decorates a <see cref="IBytesWriter"/> to log write actions.
    /// </summary>
    internal class BytesWriterLogDecorator : IBytesWriter
    {
        private readonly IBytesWriter _decoratee;
        private readonly ILogger _logger;
        private readonly string _logPrefix;

        public BytesWriterLogDecorator(IBytesWriter decoratee, ILogger logger, string logPrefix)
        {
            Guard.NotNull(decoratee, nameof(decoratee));
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhiteSpace(logPrefix, nameof(logPrefix));

            _decoratee = decoratee;
            _logger = logger;
            _logPrefix = logPrefix;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.Trace($"{_logPrefix} (offset:{offset}, count:{count}) content:{Encoding.UTF8.GetString(buffer, offset, count)}");
            }

            _decoratee.Write(buffer, offset, count);
        }
    }
}
