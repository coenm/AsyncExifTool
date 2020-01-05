namespace CoenM.ExifToolLibTest.Logging
{
    using System;

    using CoenM.ExifToolLib.Logging;
    using FluentAssertions;
    using Xunit;

    public class NullLoggerTest
    {
        [Theory]
        [ClassData(typeof(AllLogLevels))]
        public void IsEnabled_ShouldReturnFalse_ForEachLogLevel(LogLevel logLevel)
        {
            // arrange
            var sut = NullLogger.Instance;

            // act
            var result = sut.IsEnabled(logLevel);

            // assert
            result.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(AllLogLevels))]
        public void Log_ShouldNotThrow_WhenExceptionIsNull_ForEachLogLevel(LogLevel logLevel)
        {
            // arrange
            var sut = NullLogger.Instance;

            // act
            Action act = () => sut.Log(new LogEntry(logLevel, "dummy"));

            // assert
            act.Should().NotThrow();
        }

        [Theory]
        [ClassData(typeof(AllLogLevels))]
        public void Log_ShouldNotThrow_WhenExceptionIsSet_ForEachLogLevel(LogLevel logLevel)
        {
            // arrange
            var ex = new Exception("Dummy");
            var sut = NullLogger.Instance;

            // act
            Action act = () => sut.Log(new LogEntry(logLevel, "dummy", ex));

            // assert
            act.Should().NotThrow();
        }
    }
}
