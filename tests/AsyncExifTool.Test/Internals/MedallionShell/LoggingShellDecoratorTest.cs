namespace CoenM.ExifToolLibTest.Internals.MedallionShell
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals;
    using CoenM.ExifToolLib.Internals.MedallionShell;
    using CoenM.ExifToolLib.Logging;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class LoggingShellDecoratorTest
    {
        private readonly IShell decoratee;
        private readonly ILogger logger;
        private readonly LoggingShellDecorator sut;
        private readonly List<LogEntry> loggedEntries = new List<LogEntry>();

        public LoggingShellDecoratorTest()
        {
            decoratee = A.Fake<IShell>();
            logger = A.Fake<ILogger>();
            sut = new LoggingShellDecorator(decoratee, logger);

            A.CallTo(() => logger.Log(A<LogEntry>._))
                .Invokes(call => loggedEntries.Add(call.Arguments[0] as LogEntry? ?? default));

            A.CallTo(() => logger.IsEnabled(LogLevel.Debug)).Returns(false);
            A.CallTo(() => logger.IsEnabled(LogLevel.Info)).Returns(false);
            A.CallTo(() => logger.IsEnabled(LogLevel.Warn)).Returns(false);
            A.CallTo(() => logger.IsEnabled(LogLevel.Error)).Returns(false);
            A.CallTo(() => logger.IsEnabled(LogLevel.Fatal)).Returns(false);
        }

        [Fact]
        public void Initialize_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            sut.Initialize();

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => decoratee.Initialize()).MustHaveHappenedOnceExactly());

            loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Start initialising shell"));
        }

        [Fact]
        public void Initialize_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            sut.Initialize();

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => decoratee.Initialize()).MustHaveHappenedOnceExactly());
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            loggedEntries.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TryCancelAsync_ShouldLog_WhenEnabled(bool decorateeTryCancelSucceeded)
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(true);
            A.CallTo(() => decoratee.TryCancelAsync()).Returns(Task.FromResult(decorateeTryCancelSucceeded));

            // act
            var result = await sut.TryCancelAsync();

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened())
                .Then(A.CallTo(() => decoratee.TryCancelAsync()).MustHaveHappenedOnceExactly());

            loggedEntries.Should().BeEquivalentTo(
                new LogEntry(LogLevel.Trace, "TryCancel the current shell process"),
                new LogEntry(LogLevel.Trace, $"TryCancel returned {decorateeTryCancelSucceeded}."));

            result.Should().Be(decorateeTryCancelSucceeded);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TryCancelAsync_ShouldNotLog_WhenDisabled(bool decorateeTryCancelSucceeded)
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(false);
            A.CallTo(() => decoratee.TryCancelAsync()).Returns(Task.FromResult(decorateeTryCancelSucceeded));

            // act
            var result = await sut.TryCancelAsync();

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => decoratee.TryCancelAsync()).MustHaveHappenedOnceExactly());
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            loggedEntries.Should().BeEmpty();
            result.Should().Be(decorateeTryCancelSucceeded);
        }

        [Fact]
        public async Task WriteLineAsync_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            await sut.WriteLineAsync("test text");

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened())
                .Then(A.CallTo(() => decoratee.WriteLineAsync("test text")).MustHaveHappenedOnceExactly());

            loggedEntries.Should().BeEquivalentTo(
                new LogEntry(LogLevel.Trace, "WriteLineAsync: test text"));
        }

        [Fact]
        public async Task WriteLineAsync_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            await sut.WriteLineAsync("test text");

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => decoratee.WriteLineAsync("test text")).MustHaveHappenedOnceExactly());
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            loggedEntries.Should().BeEmpty();
        }

        [Fact]
        public void Kill_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            sut.Kill();

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened())
                .Then(A.CallTo(() => decoratee.Kill()).MustHaveHappenedOnceExactly());

            loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Killing shell"));
        }

        [Fact]
        public void Kill_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            sut.Kill();

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => decoratee.Kill()).MustHaveHappenedOnceExactly());
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            loggedEntries.Should().BeEmpty();
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionAdd_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            sut.ProcessExited += SutOnProcessExited;

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened());

            loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Added shells ProcessExited event handler."));
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionAdd_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            sut.ProcessExited += SutOnProcessExited;

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            loggedEntries.Should().BeEmpty();
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionRemove_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            sut.ProcessExited -= SutOnProcessExited;

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappened());

            loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Removed shells ProcessExited event handler."));
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionRemove_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            sut.ProcessExited -= SutOnProcessExited;

            // assert
            A.CallTo(() => logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            loggedEntries.Should().BeEmpty();
        }

        private void SutOnProcessExited(object? sender, EventArgs e)
        {
            return;
        }
    }
}
