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
        private readonly ILogger logger;
        private readonly List<LogEntry> logEntries;
        private readonly Exception exception;

        public LoggerExtensionsFatalTest()
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
        public void Fatal_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Fatal(logger, "test message", exception);

            // assert
            A.CallTo(() => logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Fatal, "test message", exception));
        }

        [Fact]
        public void Fatal_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Fatal)).Returns(true);

            // act
            Sut.Fatal(logger, TestMessageGenerator, exception);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Fatal)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Fatal, "Test message generated", exception));
            called.Should().BeTrue();
        }

        [Fact]
        public void Fatal_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Fatal)).Returns(false);

            // act
            Sut.Fatal(logger, TestMessageGenerator, exception);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Fatal)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Fatal_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Fatal(logger, (Func<string>)null, exception);

            // assert
            A.CallTo(logger).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
        }
    }
}
