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
        private readonly ILogger logger;
        private readonly List<LogEntry> logEntries;

        public LoggerExtensionsDebugTest()
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
        public void Debug_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).Returns(true);

            // act
            Sut.Debug(logger, "test message");

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());

            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Debug, "test message"));
        }

        [Fact]
        public void Debug_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).Returns(false);

            // act
            Sut.Debug(logger, "test message");

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
        }

        [Fact]
        public void Debug_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).Returns(true);

            // act
            Sut.Debug(logger, TestMessageGenerator);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Debug, "Test message generated"));
            called.Should().BeTrue();
        }

        [Fact]
        public void Debug_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).Returns(false);

            // act
            Sut.Debug(logger, TestMessageGenerator);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Debug_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Debug(logger, (Func<string>)null);

            // assert
            A.CallTo(logger).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
        }
    }
}
