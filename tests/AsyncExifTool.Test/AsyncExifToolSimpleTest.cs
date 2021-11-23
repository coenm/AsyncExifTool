namespace CoenM.ExifToolLibTest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CoenM.ExifToolLib;
    using CoenM.ExifToolLib.Internals;
    using CoenM.ExifToolLib.Logging;
    using CoenM.ExifToolLibTest.TestInternals;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class AsyncExifToolSimpleTest
    {
        private readonly AsyncExifTool _sut;
        private readonly TestableAsyncFakeExifTool _testSut;
        private readonly IShell _shell;

        public AsyncExifToolSimpleTest()
        {
            _shell = A.Fake<IShell>();
            _sut = _testSut = new TestableAsyncFakeExifTool(_shell);

            A.CallTo(() => _shell.WriteLineAsync(A<string>._)).Returns(Task.CompletedTask);
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void Ctor_ShouldThrow_WhenConfigurationIsNull()
        {
            // arrange
            AsyncExifToolConfiguration configuration = null;

            // act
            Action act = () => _ = new AsyncExifTool(configuration);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void Ctor_ShouldThrow_WhenLoggerIsNull()
        {
            // arrange
            ILogger logger = null;

            // act
            Action act = () => _ = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create(), logger);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Initialize_ShouldDoNothing_WhenAlreadyInitialized()
        {
            // arrange
            _sut.Initialize();

            // assume
            _testSut.CreateShellCalled.Should().Be(1);

            // act
            _sut.Initialize();

            // assert
            _testSut.CreateShellCalled.Should().Be(1);
        }

        [Fact]
        public async Task Initialize_ShouldDoNothing_WhenAlreadyInitializing()
        {
            // arrange
            ExifToolCommandControlToken controlToken = _testSut.SetupCreateShellControl();
            var firstInitializingTask = Task.Run(() => _sut.Initialize());

            // act
            controlToken.Entered.WaitOne();
            var secondInitializingTask = Task.Run(() => _sut.Initialize());

            await Task.Delay(100);
            controlToken.Release.Set();

            await firstInitializingTask;
            await secondInitializingTask;

            // assert
            _testSut.CreateShellCalled.Should().Be(1);
        }

        [Fact]
        public void Initialize_ShouldThrow_WhenShellThrows()
        {
            // arrange
            A.CallTo(() => _shell.Initialize())
             .Throws(() => new ApplicationException("Dummy test exception"));

            // act
            Action act = () => _sut.Initialize();

            // assert
            act.Should().ThrowExactly<AsyncExifToolInitialisationException>();
        }

        [Fact]
        public void ExecuteAsync_ShouldThrow_WhenNotInitialized()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await _sut.ExecuteAsync(null).ConfigureAwait(false);

            // assert
            act.Should().Throw<Exception>().WithMessage("Not initialized");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenDisposing()
        {
            // arrange
            ExifToolCommandControlToken control = _testSut.SetupControl(ExifToolArguments.BOOL_FALSE);
            _sut.Initialize();
            var disposingTask = _sut.DisposeAsync();

            // act
            control.Entered.WaitOne();
            Func<Task> act = async () => _ = await _sut.ExecuteAsync(null);

            // assert
            act.Should().Throw<Exception>().WithMessage("Disposing");

            control.Release.Set();
            _shell.ProcessExited += Raise.WithEmpty();
            await disposingTask;
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenDisposed()
        {
            // arrange

            // ExifToolArguments.StayOpen, ExifToolArguments.BoolFalse
            A.CallTo(() => _shell.WriteLineAsync(ExifToolArguments.BOOL_FALSE))
                .ReturnsLazily(async call =>
                {
                    await Task.Yield();
                    _shell.ProcessExited += Raise.WithEmpty();
                });

            _sut.Initialize();
            await _sut.DisposeAsync();

            // act
            Func<Task> act = async () => _ = await _sut.ExecuteAsync(null);

            // assert
            act.Should().Throw<Exception>().WithMessage("Disposed");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPassArgsToInnerShellAndFinishWihExecute()
        {
            // arrange
            var args = new[] { "abc", "def" };
            _sut.Initialize();

            // act
            _ = await _sut.ExecuteAsync(args, CancellationToken.None);

            // assert
            A.CallTo(() => _shell.WriteLineAsync("abc")).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _shell.WriteLineAsync("def")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => _shell.WriteLineAsync("-execute1")).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnWithStreamResult()
        {
            // arrange
            var args = new[] { "abc", "def" };
            _sut.Initialize();

            // act
            var result = await _sut.ExecuteAsync(args, CancellationToken.None);

            // assert
            result.Should().Be($"fake result abc{Environment.NewLine}fake result def" + Environment.NewLine);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFinishAllRequestsAlthoughOneWasCancelled()
        {
            // arrange
            ExifToolCommandControlToken controlExecute1 = _testSut.SetupControl("-execute1");
            var cts = new CancellationTokenSource();
            _sut.Initialize();

            // act
            Task<string> resultTask1 = _sut.ExecuteAsync(new[] { "a" }, CancellationToken.None);
            controlExecute1.Entered.WaitOne(); // make sure resultTask1 is executing and has 'entered' ExifTool
            Task<string> resultTask2 = _sut.ExecuteAsync(new[] { "b" }, cts.Token);
            Task<string> resultTask3 = _sut.ExecuteAsync(new[] { "c" }, CancellationToken.None);
            cts.Cancel(); // cancel second task
            controlExecute1.Release.Set(); // signal 'exiftool' to finish the request of task1.

            var result1 = await resultTask1;
            Func<Task> result2 = async () => await resultTask2;
            var result3 = await resultTask3;

            // assert
            result1.Should().Be("fake result a" + Environment.NewLine);
            result2.Should().ThrowExactly<TaskCanceledException>();
            result3.Should().Be("fake result c" + Environment.NewLine);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPassArgumentsToExiftoolUnlessCommandWasCancelled()
        {
            // arrange
            ExifToolCommandControlToken controlExecute1 = _testSut.SetupControl("-execute1");
            var cts = new CancellationTokenSource();
            _sut.Initialize();

            // act
            Task<string> resultTask1 = _sut.ExecuteAsync(new[] { "a", }, CancellationToken.None);
            controlExecute1.Entered.WaitOne(); // make sure resultTask1 is executing and has 'entered' ExifTool
            Task<string> resultTask2 = _sut.ExecuteAsync(new[] { "b", }, cts.Token);
            Task<string> resultTask3 = _sut.ExecuteAsync(new[] { "c", }, CancellationToken.None);
            cts.Cancel(); // cancel second task
            controlExecute1.Release.Set(); // signal 'exiftool' to finish the request of task1.

            await resultTask1;
            Func<Task> result2 = async () => await resultTask2;
            await resultTask3;

            // assert
            result2.Should().ThrowExactly<TaskCanceledException>();
            A.CallTo(() => _shell.WriteLineAsync("a")).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _shell.WriteLineAsync("-execute1")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => _shell.WriteLineAsync("c")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => _shell.WriteLineAsync("-execute2")).MustHaveHappenedOnceExactly());

            A.CallTo(() => _shell.WriteLineAsync("b")).MustNotHaveHappened();
            A.CallTo(() => _shell.WriteLineAsync("-execute3")).MustNotHaveHappened();
        }

        private class TestableAsyncFakeExifTool : AsyncExifTool
        {
            private readonly IShell _shell;
            private readonly ConcurrentDictionary<string, ExifToolCommandControlToken> _exiftoolControl;
            private Stream _exifToolStream;

            public TestableAsyncFakeExifTool(IShell shell)
                : base(AsyncExifToolConfigurationFactory.Create())
            {
                _shell = shell;
                _exiftoolControl = new ConcurrentDictionary<string, ExifToolCommandControlToken>();
                CreateShellCalled = 0;
            }

            public int CreateShellCalled { get; private set; }

            public ExifToolCommandControlToken SetupControl(string key)
            {
                var control = new ExifToolCommandControlToken();
                _exiftoolControl.TryAdd(key, control);
                return control;
            }

            public ExifToolCommandControlToken SetupCreateShellControl()
            {
                var control = new ExifToolCommandControlToken();
                _exiftoolControl.TryAdd(nameof(CreateShell), control);
                return control;
            }

            internal override IShell CreateShell(
                string exifToolFullPath,
                IEnumerable<string> args,
                Stream outputStream,
                Stream errorStream)
            {
                _exifToolStream = Stream.Synchronized(outputStream);

                CreateShellCalled++;
                if (_exiftoolControl.TryGetValue(nameof(CreateShell), out ExifToolCommandControlToken control))
                {
                    control.Entered.Set();
                    control.Release.WaitOne(TimeSpan.FromMinutes(1));
                }

                A.CallTo(() => _shell.WriteLineAsync(A<string>._))
                    .Invokes(async call =>
                    {
                        if (!(call.Arguments[0] is string text))
                        {
                            throw new ArgumentNullException(nameof(text));
                        }

                        if (_exiftoolControl.TryGetValue(text, out ExifToolCommandControlToken control))
                        {
                            control.Entered.Set();
                            await control.Release.AsTask();
                        }

                        if (text.StartsWith("-execute"))
                        {
                            var result = text.Replace("-execute", "{ready") + "}";

                            var data = Encoding.UTF8.GetBytes(result + Environment.NewLine);
#if NETCOREAPP3_0
                            await exifToolStream.WriteAsync(data);
#else
                            await _exifToolStream.WriteAsync(data, 0, data.Length);
#endif
                            return;
                        }

                        var dataFakeResult = Encoding.UTF8.GetBytes($"fake result {text}{Environment.NewLine}");
#if NETCOREAPP3_0
                        await exifToolStream.WriteAsync(dataFakeResult);
#else
                        await _exifToolStream.WriteAsync(dataFakeResult, 0, dataFakeResult.Length);
#endif

                    });

                return _shell;
            }
        }

        private class ExifToolCommandControlToken
        {
            public ExifToolCommandControlToken()
            {
                Entered = new ManualResetEvent(false);
                Release = new ManualResetEvent(false);
            }

            public ManualResetEvent Entered { get; }

            public ManualResetEvent Release { get; }
        }
    }
}
