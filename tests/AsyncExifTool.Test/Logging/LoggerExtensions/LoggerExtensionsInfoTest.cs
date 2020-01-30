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
        private readonly ILogger logger;
        private readonly List<LogEntry> logEntries;

        public LoggerExtensionsInfoTest()
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
        public void Info_ShouldAlwaysPassMessageToLogger()
        {
            // arrange

            // act
            Sut.Info(logger, "test message");

            // assert
            A.CallTo(() => logger.IsEnabled(A<LogLevel>._)).MustNotHaveHappened();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Info, "test message"));
        }

        [Fact]
        public void Info_WithLambdaExpression_ShouldPassMessageToLogger_WhenLogLevelIsEnabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Info)).Returns(true);

            // act
            Sut.Info(logger, TestMessageGenerator);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Info)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly());
            logEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Info, "Test message generated"));
            called.Should().BeTrue();
        }

        [Fact]
        public void Info_WithLambdaExpression_ShouldNotPassMessageToLogger_WhenLogLevelIsDisabled()
        {
            // arrange
            bool called = false;
            string TestMessageGenerator()
            {
                called = true;
                return "Test message generated";
            }

            A.CallTo(() => logger.IsEnabled(LogLevel.Info)).Returns(false);

            // act
            Sut.Info(logger, TestMessageGenerator);

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Info)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
            called.Should().BeFalse();
        }

        [Fact]
        public void Info_ShouldReturn_WhenMessageGeneratorIsNull()
        {
            // arrange

            // act
            Sut.Info(logger, (Func<string>)null);

            // assert
            A.CallTo(logger).MustNotHaveHappened();
            logEntries.Should().BeEmpty();
        }
    }
}
