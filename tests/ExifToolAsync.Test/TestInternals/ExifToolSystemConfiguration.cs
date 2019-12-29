namespace CoenM.ExifToolLibTest.TestInternals
{
    using System;
    using System.IO;
    using System.Reflection;

    using TestHelper;

    internal static class ExifToolSystemConfiguration
    {
        private const string ExiftoolVersion = "EXIFTOOL_VERSION";
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
        private static readonly string EmbeddedResourceNs = "ExifToolAsyncTest"; // typeof(ExifToolSystemConfiguration).Namespace;
        private static readonly Lazy<string> GetConfigExiftoolVersionImpl = new Lazy<string>(GetConfiguredExiftoolVersion);
        private static readonly Lazy<string> GetExifToolExecutableImpl = new Lazy<string>(GetExifToolExecutable);

        public static string ConfiguredVersion => GetConfigExiftoolVersionImpl.Value;

        public static string ExifToolExecutable => GetExifToolExecutableImpl.Value;

        private static string GetExifToolExecutable()
        {
            var osFilename = OperatingSystemHelper.IsLinuxOrMacOsx ? "exiftool" : "exiftool.exe";

            // first try to grab local Exiftool, otherwise assume global exiftool
            var fullFilename = TestEnvironment.GetFullPath("tools", osFilename);
            if (File.Exists(fullFilename))
                return fullFilename;

            return osFilename;
        }

        private static string GetConfiguredExiftoolVersion()
        {
            using var stream = OpenRead();
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd().Trim();
        }

        private static Stream OpenRead()
        {
            return Assembly.GetManifestResourceStream(EmbeddedResourceNs + "." + ExiftoolVersion);
        }
    }
}
