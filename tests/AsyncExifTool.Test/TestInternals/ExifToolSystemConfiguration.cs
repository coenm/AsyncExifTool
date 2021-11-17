namespace CoenM.ExifToolLibTest.TestInternals
{
    using System;
    using System.IO;
    using System.Reflection;
    using TestHelper;

    internal static class ExifToolSystemConfiguration
    {
        private const string EXIFTOOL_VERSION = "EXIFTOOL_VERSION";
        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private static readonly string _embeddedResourceNs = "ExifToolAsyncTest"; // typeof(ExifToolSystemConfiguration).Namespace;
        private static readonly Lazy<string> _getConfigExiftoolVersionImpl = new Lazy<string>(GetConfiguredExiftoolVersion);
        private static readonly Lazy<string> _getExifToolExecutableImpl = new Lazy<string>(GetExifToolExecutable);
        private static readonly Lazy<string> _getExifCustomConfigFileImpl = new Lazy<string>(GetExifCustomConfigFile);

        public static string ConfiguredVersion => _getConfigExiftoolVersionImpl.Value;

        public static string ExifToolExecutable => _getExifToolExecutableImpl.Value;

        public static string CustomExifToolConfigFile => _getExifCustomConfigFileImpl.Value;

        private static string GetExifToolExecutable()
        {
            var osFilename = OperatingSystemHelper.IsLinuxOrMacOsx ? "exiftool" : "exiftool.exe";

            // first try to grab local Exiftool, otherwise assume global exiftool
            var fullFilename = TestEnvironment.GetFullPath("tools", osFilename);
            if (File.Exists(fullFilename))
            {
                return fullFilename;
            }

            return osFilename;
        }

        private static string GetExifCustomConfigFile()
        {
            const string CONFIG_FILENAME = "AsyncExifTool.ExifTool_config";

            var fullFilename = TestEnvironment.GetFullPath("tests", CONFIG_FILENAME);
            if (File.Exists(fullFilename))
            {
                return fullFilename;
            }

            return CONFIG_FILENAME;
        }

        private static string GetConfiguredExiftoolVersion()
        {
            using Stream stream = OpenRead();
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd().Trim();
        }

        private static Stream OpenRead()
        {
            return _assembly.GetManifestResourceStream(_embeddedResourceNs + "." + EXIFTOOL_VERSION);
        }
    }
}
