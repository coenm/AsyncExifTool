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
    public class LoggerExtensionsFatalTest
    {
        private readonly ILogger _logger;
        private readonly List<LogEntry> _logEntries;
        private readonly Exception _exception;

        public LoggerExtensionsFatalTest()
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
        public void Fatal_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Fatal(_logger, "test message", _exception);

            // assert
            A.CallTo(() => _logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Fatal, "test message", _exception));
        }

        [Fact]
        public void Fatal_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Fatal)).Returns(true);

            // act
            Sut.Fatal(_logger, TestMessageGenerator, _exception);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Fatal)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Fatal, "Test message generated", _exception));
            called.Should().BeTrue();
        }

        [Fact]
        public void Fatal_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Fatal)).Returns(false);

            // act
            Sut.Fatal(_logger, TestMessageGenerator, _exception);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Fatal)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Fatal_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Fatal(_logger, (Func<string>)null, _exception);

            // assert
            A.CallTo(_logger).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
        }
    }
}
