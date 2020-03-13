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
        [NotNull] private readonly IBytesWriter decoratee;
        [NotNull] private readonly ILogger logger;
        [NotNull] private readonly string logPrefix;

        public BytesWriterLogDecorator([NotNull] IBytesWriter decoratee, [NotNull] ILogger logger, [NotNull]string logPrefix)
        {
            Guard.NotNull(decoratee, nameof(decoratee));
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNullOrWhiteSpace(logPrefix, nameof(logPrefix));

            this.decoratee = decoratee;
            this.logger = logger;
            this.logPrefix = logPrefix;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (logger.IsEnabled(LogLevel.Trace))
                logger.Trace($"{logPrefix} (offset:{offset}, count:{count}) content:{Encoding.UTF8.GetString(buffer, offset, count)}");

            decoratee.Write(buffer, offset, count);
        }
    }
}
