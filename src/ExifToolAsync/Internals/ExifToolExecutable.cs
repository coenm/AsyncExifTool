namespace ExifToolAsync.Internals
{
    using System.Runtime.InteropServices;
    
    using JetBrains.Annotations;

    internal static class ExifToolExecutable
    {
        private const string WindowsEol = "\r\n";

        private const string LinuxEol = "\n";

        [PublicAPI]
        public static bool IsLinuxOrMacOsx => !IsWindows;

        [PublicAPI]
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        [PublicAPI]
        public static string NewLine => IsWindows ? WindowsEol : LinuxEol;
    }
}
