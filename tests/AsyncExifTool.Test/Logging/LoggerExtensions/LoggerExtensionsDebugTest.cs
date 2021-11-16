namespace CoenM.ExifToolLibTest.Logging.LoggerExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CoenM.ExifToolLib.Logging;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;
    using Sut = CoenM.ExifToolLib.Logging.LoggerExtensions;

    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod", Justification = "Improve Sut visibility")]
    public class LoggerExtensionsDebugTest
    {
        private readonly ILogger _logger;
        private readonly List<LogEntry> _logEntries;

        public LoggerExtensionsDebugTest()
        {
            _logEntries = new List<LogEntry>();

            _logger = A.Fake<ILogger>();
            A.CallTo(() => _logger.Log(A<LogEntry>._))
                .Invokes(call =>
                {
                    if (!(call.Arguments[0] is LogEntry logEntry))
                    {
                        return;
                    }

                    _logEntries.Add(logEntry);
                });
        }

        [Fact]
        public void Debug_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Debug(_logger, "test message");

            // assert
            A.CallTo(() => _logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Debug, "test message"));
        }

        [Fact]
        public void Debug_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Debug)).Returns(true);

            // act
            Sut.Debug(_logger, TestMessageGenerator);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Debug)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Debug, "Test message generated"));
            called.Should().BeTrue();
        }

        [Fact]
        public void Debug_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Debug)).Returns(false);

            // act
            Sut.Debug(_logger, TestMessageGenerator);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Debug)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Debug_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Debug(_logger, (Func<string>)null);

            // assert
            A.CallTo(_logger).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
        }
    }
}
