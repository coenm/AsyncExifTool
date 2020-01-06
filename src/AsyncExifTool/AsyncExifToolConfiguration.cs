namespace CoenM.ExifToolLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using JetBrains.Annotations;

    public sealed class AsyncExifToolConfiguration
    {
        public AsyncExifToolConfiguration(
            [NotNull] string exifToolFullFilename,
            [NotNull] Encoding exifToolResultEncoding,
            [NotNull] string exifToolNewLine,
            [CanBeNull] IReadOnlyCollection<string> commonArgs)
        {
            if (string.IsNullOrWhiteSpace(exifToolFullFilename))
                throw new ArgumentNullException(nameof(exifToolFullFilename));
            ExifToolFullFilename = exifToolFullFilename;

            ExifToolEncoding = exifToolResultEncoding ?? throw new ArgumentNullException(nameof(exifToolResultEncoding));

            CommonArgs = commonArgs == null
                ? new List<string>(0)
                : commonArgs.Where(item => item != null).ToList();

            if (string.IsNullOrEmpty(exifToolNewLine))
                throw new ArgumentNullException(nameof(exifToolNewLine));
            ExifToolNewLine = exifToolNewLine;
        }

        /// <summary>
        /// Full path to ExifTool executable.
        /// </summary>
        public string ExifToolFullFilename { get; }

        /// <summary>
        /// NewLine characters. For windows, this is '\r\n', Unix uses '\n'.
        /// </summary>
        public string ExifToolNewLine { get; }

        /// <summary>
        /// Expected ExifTool encoding.
        /// </summary>
        public Encoding ExifToolEncoding { get; }

        /// <summary>
        /// Common arguments.
        /// </summary>
        public List<string> CommonArgs { get; }
    }
}
