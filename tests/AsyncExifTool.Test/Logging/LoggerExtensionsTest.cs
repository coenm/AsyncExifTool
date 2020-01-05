namespace CoenM.ExifToolLibTest.Logging
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using CoenM.ExifToolLib.Logging;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    using Sut = CoenM.ExifToolLib.Logging.LoggerExtensions;

    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod", Justification = "Improve Sut visibility")]
    public class LoggerExtensionsTest
    {
        private readonly ILogger logger;
        private readonly List<LogEntry> logEntries;

        public LoggerExtensionsTest()
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
        public void Debug_ShouldPassMessageToLogger_WhenDebugLevelIsEnabled()
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
    }
}
