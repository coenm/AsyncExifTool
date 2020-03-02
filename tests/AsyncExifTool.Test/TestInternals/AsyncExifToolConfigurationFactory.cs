﻿namespace CoenM.ExifToolLibTest.TestInternals
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
                                                  new List<string> { ExifToolArguments.CommonArgs, });
        }
    }
}
