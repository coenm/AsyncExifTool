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
    public class LoggerExtensionsWarnTest
    {
        private readonly ILogger logger;
        private readonly List<LogEntry> logEntries;

        public LoggerExtensionsWarnTest()
        {
            logEntries = new List<LogEntry>();

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
        public void Warn_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).Returns(true);

            // act
            Sut.Warn(logger, "test message");

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());

            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Warn, "test message"));
        }

        [Fact]
        public void Warn_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).Returns(false);

            // act
            Sut.Warn(logger, "test message");

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
        }

        [Fact]
        public void Warn_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).Returns(true);

            // act
            Sut.Warn(logger, TestMessageGenerator);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Warn, "Test message generated"));
            called.Should().BeTrue();
        }

        [Fact]
        public void Warn_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).Returns(false);

            // act
            Sut.Warn(logger, TestMessageGenerator);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Info_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Warn(logger, (Func<string>)null);

            // assert
            A.CallTo(logger).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
        }
    }
}
