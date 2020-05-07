namespace CoenM.ExifToolLibTest.TestInternals
{
    using System.Collections.Generic;
    using System.Text;

    using CoenM.ExifToolLib;
    using CoenM.ExifToolLib.Internals;

    internal static class AsyncExifToolConfigurationFactory
    {
        public static AsyncExifToolConfiguration Create()
        {
            return new AsyncExifToolConfiguration(
                                                  ExifToolSystemConfiguration.ExifToolExecutable,
                                                  Encoding.UTF8,
                                                  null,
                                                  new List<string> { ExifToolArguments.CommonArgs, });
        }

        public static AsyncExifToolConfiguration CreateWithCustomConfig()
        {
            return new AsyncExifToolConfiguration(
                ExifToolSystemConfiguration.ExifToolExecutable,
                Encoding.UTF8,
                ExifToolSystemConfiguration.CustomExifToolConfigFile,
                new List<string> { ExifToolArguments.CommonArgs, });
        }
    }
}
