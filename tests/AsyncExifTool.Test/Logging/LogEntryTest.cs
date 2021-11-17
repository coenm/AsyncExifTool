namespace CoenM.ExifToolLibTest.Logging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using CoenM.ExifToolLib.Logging;
    using FluentAssertions;
    using Xunit;

    public class LogEntryTest
    {
        [Fact]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1129:Do not use default value type constructor", Justification = "Testcase")]
        public void LogEntry_DefaultCtor_ShouldSetAllPropertiesToDefault()
        {
            // arrange

            // act
            var sut = new LogEntry();

            // assert
            sut.Exception.Should().Be(default(Exception));
            sut.Message.Should().Be(default);
            sut.Severity.Should().Be(default(LogLevel));
        }

        [Theory]
        [ClassData(typeof(AllLogLevels))]
        public void LogEntry_ShouldReflectLogLevel_WhenConstructed(LogLevel logLevel)
        {
            // arrange

            // act
            var sut = new LogEntry(logLevel, "dummy", null);

            // assert
            sut.Severity.Should().Be(logLevel);

            sut.Message.Should().Be("dummy");
            sut.Exception.Should().BeNull();
        }

        [Theory]
        [ClassData(typeof(AllLogLevels))]
        public void LogEntry_ShouldReflectException_WhenConstructed(LogLevel logLevel)
        {
            // arrange
            var exception = new ApplicationException("bla");

            // act
            var sut = new LogEntry(logLevel, "dummy1", exception);

            // assert
            sut.Exception.Should().Be(exception);

            sut.Severity.Should().Be(logLevel);
            sut.Message.Should().Be("dummy1");
        }

        [Theory]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        [ClassData(typeof(AllLogLevels))]
        public void LogEntry_ShouldThrow_WhenMessageIsNull(LogLevel logLevel)
        {
            // arrange
            string message = null;

            // act
            Action act = () => _ = new LogEntry(logLevel, message, null);

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [ClassData(typeof(AllLogLevels))]
        public void LogEntry_ShouldThrow_WhenMessageIsEmptyString(LogLevel logLevel)
        {
            // arrange
            var message = "   ";

            // act
            Action act = () => _ = new LogEntry(logLevel, message, null);

            // assert
            act.Should().ThrowExactly<ArgumentException>();
        }
    }
}
