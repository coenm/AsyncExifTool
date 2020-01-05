namespace CoenM.ExifToolLibTest.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CoenM.ExifToolLib.Logging;
    using Xunit;

    internal class AllLogLevels : TheoryData<LogLevel>
    {
        private static readonly IEnumerable<LogLevel> AllItems = Enum
                                                                 .GetValues(typeof(LogLevel))
                                                                 .Cast<LogLevel>()
                                                                 .AsEnumerable();

        public AllLogLevels()
        {
            foreach (var logLevel in AllItems)
                Add(logLevel);
        }
    }
}
