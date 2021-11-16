namespace CoenM.ExifToolLibTest.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CoenM.ExifToolLib.Logging;
    using Xunit;

    internal class AllLogLevels : TheoryData<LogLevel>
    {
        private static readonly IEnumerable<LogLevel> _allItems = Enum
                                                                 .GetValues(typeof(LogLevel))
                                                                 .Cast<LogLevel>()
                                                                 .AsEnumerable();

        public AllLogLevels()
        {
            foreach (LogLevel logLevel in _allItems)
            {
                Add(logLevel);
            }
        }
    }
}
