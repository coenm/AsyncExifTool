namespace CoenM.ExifToolLib.Internals
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    internal static class ExifToolArguments
    {
        public const string VERSION = "-ver";
        public const string STAY_OPEN = "-stay_open";
        public const string BOOL_TRUE = "True";
        public const string BOOL_FALSE = "False";
        public const string COMMON_ARGS = "-common_args";

        /// <summary>
        /// Keep reading -@ argfile even after EOF.
        /// </summary>
        /// <param name="enabled">boolean variable indicating to enable or disable the 'stay_open' functionality.</param>
        /// <returns>Exiftool arguments.</returns>
        public static IEnumerable<string> StayOpenMode(bool enabled)
        {
            yield return STAY_OPEN;
            yield return enabled ? BOOL_TRUE : BOOL_FALSE;
        }

        /// <summary>
        /// Keep reading commands from stdin.
        /// </summary>
        /// <returns>Exiftool arguments.</returns>
        public static IEnumerable<string> ReadCommandLineArgumentsFromStdIn()
        {
            // Read command-line arguments from file.
            yield return "-@";

            // argument file is std in
            yield return "-";
        }

        /// <summary>
        /// Use custom configuration file.
        /// </summary>
        /// <param name="filename">filename to configuration file.</param>
        /// <returns>Exiftool arguments.</returns>
        public static IEnumerable<string> ExifToolArgumentsConfigFile(string? filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                yield break;
            }

            yield return "-config";
            yield return filename!;
        }
    }
}
