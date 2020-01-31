namespace CoenM.ExifToolLib.Internals
{
    using System.Collections.Generic;

    internal static class ExifToolArguments
    {
        public const string Version = "-ver";
        public const string StayOpen = "-stay_open";
        public const string BoolTrue = "True";
        public const string BoolFalse = "False";
        public const string CommonArgs = "-common_args";

        /// <summary>
        /// Keep reading -@ argfile even after EOF.
        /// </summary>
        /// <param name="enabled">boolean variable indicating to enable or disable the 'stay_open' functionality.</param>
        /// <returns>Exiftool arguments.</returns>
        public static IEnumerable<string> StayOpenMode(bool enabled)
        {
            yield return StayOpen;
            yield return enabled ? BoolTrue : BoolFalse;
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
    }
}
