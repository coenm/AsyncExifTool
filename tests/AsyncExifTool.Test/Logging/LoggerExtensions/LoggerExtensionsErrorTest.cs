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
        private readonly ILogger logger;
        private readonly List<LogEntry> logEntries;
        private readonly Exception exception;

        public LoggerExtensionsErrorTest()
        {
            logEntries = new List<LogEntry>();

            exception = new Exception("test123");

            logger = A.Fake<ILogger>();
            A.CallTo(() => logger.Log(A<LogEntry>._))
                .Invokes(call =>
                {
                    if (!(call.Arguments[0] is LogEntry logEntry))
                        return;
                    logEntries.Add(logEntry);
                });
        }

        [Fact]
        public void Error_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Error(logger, "test message", exception);

            // assert
            A.CallTo(() => logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Error, "test message", exception));
        }

        [Fact]
        public void Error_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Error)).Returns(true);

            // act
            Sut.Error(logger, TestMessageGenerator, exception);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Error)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Error, "Test message generated", exception));
            called.Should().BeTrue();
        }

        [Fact]
        public void Error_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Error)).Returns(false);

            // act
            Sut.Error(logger, TestMessageGenerator, exception);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Error)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Error_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Error(logger, (Func<string>)null, exception);

            // assert
            A.CallTo(logger).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
        }
    }
}
