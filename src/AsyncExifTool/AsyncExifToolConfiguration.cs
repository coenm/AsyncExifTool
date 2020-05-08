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
        /// <param name="configurationFilename">Full path of external configuration filename. Can be null. see '-config' option in the exiftool Application documentation.</param>
        /// <param name="exifToolResultEncoding">Encoding how to decode the ExifTool output.</param>
        /// <param name="commonArgs">Define common arguments. See '-common_args' in the online ExifTool documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exifToolFullFilename"/>, or <paramref name="exifToolResultEncoding"/> is <c>null</c>.</exception>
        public AsyncExifToolConfiguration(
            [NotNull] string exifToolFullFilename,
            [CanBeNull] string configurationFilename,
            [NotNull] Encoding exifToolResultEncoding,
            [CanBeNull] IReadOnlyCollection<string> commonArgs)
        {
            Guard.NotNullOrWhiteSpace(exifToolFullFilename, nameof(exifToolFullFilename));
            Guard.NotNull(exifToolResultEncoding, nameof(exifToolResultEncoding));

            ExifToolFullFilename = exifToolFullFilename;
            ExifToolEncoding = exifToolResultEncoding;

            ConfigurationFilename = configurationFilename;

            CommonArgs = commonArgs == null
                ? new List<string>(0)
                : commonArgs.Where(item => item != null).ToList();
        }

        /// <summary>
        /// Create configuration for <see cref="AsyncExifTool"/> with no custom exiftool configuration.
        /// </summary>
        /// <param name="exifToolFullFilename">Full path to exiftool on the executing host. No checks are made in the configuration if the path is valid.</param>
        /// <param name="exifToolResultEncoding">Encoding how to decode the ExifTool output.</param>
        /// <param name="commonArgs">Define common arguments. See '-common_args' in the online ExifTool documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exifToolFullFilename"/>, or <paramref name="exifToolResultEncoding"/> is <c>null</c>.</exception>
        public AsyncExifToolConfiguration(
            [NotNull] string exifToolFullFilename,
            [NotNull] Encoding exifToolResultEncoding,
            [CanBeNull] IReadOnlyCollection<string> commonArgs)
            : this(exifToolFullFilename, null, exifToolResultEncoding, commonArgs)
        {
        }

        /// <summary>
        /// Full path to ExifTool executable.
        /// </summary>
        public string ExifToolFullFilename { get; }

        /// <summary>
        /// Load specified configuration file instead of the default ".ExifTool_config".
        /// If used, this option must come before all other arguments on the command line and applies to all executed commands. The config file must exist relative to the current working directory or the exiftool application directory unless an absolute path is specified. Loading of the default config file may be disabled by setting CFGFILE to an empty string (ie. ""). See https://exiftool.org/config.html and config_files/example.config in the full ExifTool distribution for details about the configuration file syntax.
        /// </summary>
        [CanBeNull] public string ConfigurationFilename { get; }

        /// <summary>
        /// Expected ExifTool encoding.
        /// </summary>
        public Encoding ExifToolEncoding { get; }

        /// <summary>
        /// Common arguments.
        /// </summary>
        [NotNull] public List<string> CommonArgs { get; }
    }
}
