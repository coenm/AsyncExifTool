namespace CoenM.ExifToolLib.Internals
{
    using System.Runtime.InteropServices;

    using JetBrains.Annotations;

    internal static class ExifToolExecutable
    {
        [PublicAPI]
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
