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
    public class LoggerExtensionsErrorTest
    {
        private readonly ILogger _logger;
        private readonly List<LogEntry> _logEntries;
        private readonly Exception _exception;

        public LoggerExtensionsErrorTest()
        {
            _logEntries = new List<LogEntry>();

            _exception = new Exception("test123");

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
        public void Error_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Error(_logger, "test message", _exception);

            // assert
            A.CallTo(() => _logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Error, "test message", _exception));
        }

        [Fact]
        public void Error_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Error)).Returns(true);

            // act
            Sut.Error(_logger, TestMessageGenerator, _exception);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Error)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Error, "Test message generated", _exception));
            called.Should().BeTrue();
        }

        [Fact]
        public void Error_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Error)).Returns(false);

            // act
            Sut.Error(_logger, TestMessageGenerator, _exception);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Error)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Error_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Error(_logger, (Func<string>)null, _exception);

            // assert
            A.CallTo(_logger).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
        }
    }
}
