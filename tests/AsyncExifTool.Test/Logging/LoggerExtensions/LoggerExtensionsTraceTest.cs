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
    public class LoggerExtensionsTraceTest
    {
        private readonly ILogger _logger;
        private readonly List<LogEntry> _logEntries;

        public LoggerExtensionsTraceTest()
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
        public void Trace_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Trace(_logger, "test message");

            // assert
            A.CallTo(() => _logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "test message"));
        }

        [Fact]
        public void Trace_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            Sut.Trace(_logger, TestMessageGenerator);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            _logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Test message generated"));
            called.Should().BeTrue();
        }

        [Fact]
        public void Trace_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            var called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            Sut.Trace(_logger, TestMessageGenerator);

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Trace_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Trace(_logger, (Func<string>)null);

            // assert
            A.CallTo(_logger).MustNotHaveHappened();
            _logEntries.Should().BeEmpty();
        }
    }
}
