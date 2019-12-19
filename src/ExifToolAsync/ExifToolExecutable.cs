namespace EagleEye.ExifTool
{
    using System.Runtime.InteropServices;
    using System.Text;

    using JetBrains.Annotations;

    public static class ExifToolExecutable
    {
        private const string WindowsEol = "\r\n";

        private const string LinuxEol = "\n";

        [PublicAPI]
        public static bool IsLinuxOrMacOsx => !IsWindows;

        [PublicAPI]
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        [PublicAPI]
        public static string NewLine
        {
            get
            {
                if (IsWindows)
                    return WindowsEol;
                return LinuxEol;
            }
        }

        [PublicAPI]
        public static byte[] NewLineBytes => Encoding.ASCII.GetBytes(NewLine);

        /// <summary>
        /// Convert a string with either Linux or Windows line-endings to the OS specific line-endings.
        /// </summary>
        /// <param name="input">string with or without line endings.</param>
        /// <returns>Input string with all line-endings sanitized to the OS default line-ending.</returns>
        [PublicAPI]
        public static string ConvertToOsString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var linuxSanitized = input.Replace(WindowsEol, LinuxEol);

            if (!IsWindows)
                return linuxSanitized;

            return linuxSanitized.Replace(LinuxEol, WindowsEol);
        }

        [PublicAPI]
        public static string GetExecutableName()
        {
            if (IsLinuxOrMacOsx)
                return "exiftool";
            return "exiftool.exe";
        }
    }
}
