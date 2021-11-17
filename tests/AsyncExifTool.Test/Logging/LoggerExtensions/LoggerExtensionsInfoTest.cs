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
    public class LoggerExtensionsInfoTest
    {
        private readonly ILogger _logger;
        private readonly List<LogEntry> _logEntries;

        public LoggerExtensionsInfoTest()
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
        public void Info_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Info(_logger, "test message");

            // assert
            A.CallTo(() => _logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Info, "test message"));
        }

        [Fact]
        public void Info_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Info)).Returns(true);

            // act
            Sut.Info(_logger, TestMessageGenerator);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Info)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Info, "Test message generated"));
            called.Should().BeTrue();
        }

        [Fact]
        public void Info_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Info)).Returns(false);

            // act
            Sut.Info(_logger, TestMessageGenerator);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Info)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Info_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Info(_logger, (Func<string>)null);

            // assert
            A.CallTo(_logger).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
        }
    }
}
