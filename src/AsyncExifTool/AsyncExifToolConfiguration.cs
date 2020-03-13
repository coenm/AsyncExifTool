namespace CoenM.ExifToolLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;

    /// <summary>
    /// Configuration for <see cref="AsyncExifTool"/>.
    /// </summary>
    public sealed class AsyncExifToolConfiguration
    {
        /// <summary>
        /// Create configuration for <see cref="AsyncExifTool"/>.
        /// </summary>
        /// <param name="exifToolFullFilename">Full path to exiftool on the executing host. No checks are made in the configuration if the path is valid.</param>
        /// <param name="exifToolResultEncoding">Encoding how to decode the ExifTool output.</param>
        /// <param name="commonArgs">Define common arguments. See '-common_args' in the online ExifTool documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exifToolFullFilename"/>, or <paramref name="exifToolResultEncoding"/> is <c>null</c>.</exception>
        public AsyncExifToolConfiguration(
            [NotNull] string exifToolFullFilename,
            [NotNull] Encoding exifToolResultEncoding,
            [CanBeNull] IReadOnlyCollection<string> commonArgs)
        {
            Guard.NotNullOrWhiteSpace(exifToolFullFilename, nameof(exifToolFullFilename));
            Guard.NotNull(exifToolResultEncoding, nameof(exifToolResultEncoding));

            ExifToolFullFilename = exifToolFullFilename;
            ExifToolEncoding = exifToolResultEncoding;

            CommonArgs = commonArgs == null
                ? new List<string>(0)
                : commonArgs.Where(item => item != null).ToList();
        }

        /// <summary>
        /// Full path to ExifTool executable.
        /// </summary>
        public string ExifToolFullFilename { get; }

        /// <summary>
        /// Expected ExifTool encoding.
        /// </summary>
        public Encoding ExifToolEncoding { get; }

        /// <summary>
        /// Common arguments.
        /// </summary>
        [NotNull]
        public List<string> CommonArgs { get; }
    }
}
