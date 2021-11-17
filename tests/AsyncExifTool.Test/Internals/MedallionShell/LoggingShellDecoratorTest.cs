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
        private readonly IShell _decoratee;
        private readonly ILogger _logger;
        private readonly LoggingShellDecorator _sut;
        private readonly List<LogEntry> _loggedEntries = new List<LogEntry>();

        public LoggingShellDecoratorTest()
        {
            _decoratee = A.Fake<IShell>();
            _logger = A.Fake<ILogger>();
            _sut = new LoggingShellDecorator(_decoratee, _logger);

            A.CallTo(() => _logger.Log(A<LogEntry>._))
                .Invokes(call => _loggedEntries.Add(call.Arguments[0] as LogEntry? ?? default));

            A.CallTo(() => _logger.IsEnabled(LogLevel.Debug)).Returns(false);
            A.CallTo(() => _logger.IsEnabled(LogLevel.Info)).Returns(false);
            A.CallTo(() => _logger.IsEnabled(LogLevel.Warn)).Returns(false);
            A.CallTo(() => _logger.IsEnabled(LogLevel.Error)).Returns(false);
            A.CallTo(() => _logger.IsEnabled(LogLevel.Fatal)).Returns(false);
        }

        [Fact]
        public void Initialize_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            _sut.Initialize();

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => _decoratee.Initialize()).MustHaveHappenedOnceExactly());

            _loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Start initialising shell"));
        }

        [Fact]
        public void Initialize_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            _sut.Initialize();

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _decoratee.Initialize()).MustHaveHappenedOnceExactly());
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _loggedEntries.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TryCancelAsync_ShouldLog_WhenEnabled(bool decorateeTryCancelSucceeded)
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(true);
            A.CallTo(() => _decoratee.TryCancelAsync()).Returns(Task.FromResult(decorateeTryCancelSucceeded));

            // act
            var result = await _sut.TryCancelAsync();

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappened())
                .Then(A.CallTo(() => _decoratee.TryCancelAsync()).MustHaveHappenedOnceExactly());

            _loggedEntries.Should().BeEquivalentTo(
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
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(false);
            A.CallTo(() => _decoratee.TryCancelAsync()).Returns(Task.FromResult(decorateeTryCancelSucceeded));

            // act
            var result = await _sut.TryCancelAsync();

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _decoratee.TryCancelAsync()).MustHaveHappenedOnceExactly());
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _loggedEntries.Should().BeEmpty();
            result.Should().Be(decorateeTryCancelSucceeded);
        }

        [Fact]
        public async Task WriteLineAsync_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            await _sut.WriteLineAsync("test text");

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappened())
                .Then(A.CallTo(() => _decoratee.WriteLineAsync("test text")).MustHaveHappenedOnceExactly());

            _loggedEntries.Should().BeEquivalentTo(
                new LogEntry(LogLevel.Trace, "WriteLineAsync: test text"));
        }

        [Fact]
        public async Task WriteLineAsync_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            await _sut.WriteLineAsync("test text");

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _decoratee.WriteLineAsync("test text")).MustHaveHappenedOnceExactly());
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _loggedEntries.Should().BeEmpty();
        }

        [Fact]
        public void Kill_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            _sut.Kill();

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappened())
                .Then(A.CallTo(() => _decoratee.Kill()).MustHaveHappenedOnceExactly());

            _loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Killing shell"));
        }

        [Fact]
        public void Kill_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            _sut.Kill();

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _decoratee.Kill()).MustHaveHappenedOnceExactly());
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _loggedEntries.Should().BeEmpty();
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionAdd_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            _sut.ProcessExited += SutOnProcessExited;

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappened());

            _loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Added shells ProcessExited event handler."));
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionAdd_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            _sut.ProcessExited += SutOnProcessExited;

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _loggedEntries.Should().BeEmpty();
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionRemove_ShouldLog_WhenEnabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            _sut.ProcessExited -= SutOnProcessExited;

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _logger.Log(A<LogEntry>._)).MustHaveHappened());

            _loggedEntries.Should().BeEquivalentTo(new LogEntry(LogLevel.Trace, "Removed shells ProcessExited event handler."));
        }

        [Fact]
        public void ProcessExitedEventHandlerSubscriptionRemove_ShouldNotLog_WhenDisabled()
        {
            // arrange
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            _sut.ProcessExited -= SutOnProcessExited;

            // assert
            A.CallTo(() => _logger.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            _loggedEntries.Should().BeEmpty();
        }

        [Fact]
        public async Task Task_ShouldNotLog()
        {
            // arrange
            IShellResult taskResult = A.Dummy<IShellResult>();
            A.CallTo(() => _decoratee.Task).Returns(Task.FromResult(taskResult));

            // act
            var result = await _sut.Task;

            // assert
            A.CallTo(_decoratee).MustHaveHappenedOnceExactly();
            A.CallTo(_logger).MustNotHaveHappened();
            result.Should().Be(taskResult);
        }

        [Fact]
        public void Dispose_ShouldReturn_WhenDecorateeDoesNotImplementIDisposable()
        {
            // arrange

            // act
            _sut.Dispose();

            // assert
            A.CallTo(_logger).MustNotHaveHappened();
            A.CallTo(_decoratee).MustNotHaveHappened();
            _loggedEntries.Should().BeEmpty();
        }

        private void SutOnProcessExited(object sender, EventArgs e)
        {
        }
    }
}
