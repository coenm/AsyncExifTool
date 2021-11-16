namespace TestHelper
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using JetBrains.Annotations;

    public static class OperatingSystemHelper
    {
        private const string WINDOWS_EOL = "\r\n";
        private const string LINUX_EOL = "\n";

        [PublicAPI]
        public static bool IsLinuxOrMacOsx => !IsWindows;

        [PublicAPI]
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        [PublicAPI]
        public static byte[] NewLineBytes => Encoding.ASCII.GetBytes(Environment.NewLine);

        /// <summary>
        /// Convert a string with either Linux or Windows line-endings to the OS specific line-endings.
        /// </summary>
        /// <param name="input">string with or without line endings.</param>
        /// <returns>Input string with all line-endings sanitized to the OS default line-ending.</returns>
        [PublicAPI]
        public static string ConvertToOsString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var linuxSanitized = input.Replace(WINDOWS_EOL, LINUX_EOL);

            if (!IsWindows)
            {
                return linuxSanitized;
            }

            return linuxSanitized.Replace(LINUX_EOL, WINDOWS_EOL);
        }
    }
}
