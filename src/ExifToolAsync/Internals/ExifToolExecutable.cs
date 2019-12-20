namespace ExifToolAsync.Internals
{
    using System.Runtime.InteropServices;
    
    using JetBrains.Annotations;

    internal static class ExifToolExecutable
    {
        private const string WindowsEol = "\r\n";

        private const string LinuxEol = "\n";

        private static bool IsLinuxOrMacOsx => !IsWindows;

        private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

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
    }
}
