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
    using TestHelper;
    using Xunit;

    public class AsyncExifToolSimpleTest
    {
        private readonly AsyncExifTool sut;
        private readonly TestableAsyncFakeExifTool testSut;
        private readonly IShell shell;

        public AsyncExifToolSimpleTest()
        {
            shell = A.Fake<IShell>();
            sut = testSut = new TestableAsyncFakeExifTool(shell);

            A.CallTo(() => shell.WriteLineAsync(A<string>._)).Returns(Task.CompletedTask);
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void Ctor_ShouldThrow_WhenConfigurationIsNull()
        {
            // arrange
            AsyncExifToolConfiguration configuration = null;

            // act
            Action act = () => new AsyncExifTool(configuration);

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
            Action act = () => new AsyncExifTool(AsyncExifToolConfigurationFactory.Create(), logger);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Initialize_ShouldDoNothing_WhenAlreadyInitialized()
        {
            // arrange
            sut.Initialize();

            // assume
            testSut.CreateShellCalled.Should().Be(1);

            // act
            sut.Initialize();

            // assert
            testSut.CreateShellCalled.Should().Be(1);
        }

        [Fact]
        public async Task Initialize_ShouldDoNothing_WhenAlreadyInitializing()
        {
            // arrange
            var controlToken = testSut.SetupCreateShellControl();
            var firstInitializingTask = Task.Run(() => sut.Initialize());

            // act
            controlToken.Entered.WaitOne();
            var secondInitializingTask = Task.Run(() => sut.Initialize());

            await Task.Delay(100);
            controlToken.Release.Set();

            await firstInitializingTask;
            await secondInitializingTask;

            // assert
            testSut.CreateShellCalled.Should().Be(1);
        }

        [Fact]
        public void ExecuteAsync_ShouldThrow_WhenNotInitialized()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null).ConfigureAwait(false);

            // assert
            act.Should().Throw<Exception>().WithMessage("Not initialized");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenDisposing()
        {
            // arrange
            var control = testSut.SetupControl(ExifToolArguments.BoolFalse);
            sut.Initialize();
            var disposingTask = sut.DisposeAsync();

            // act
            control.Entered.WaitOne();
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null);

            // assert
            act.Should().Throw<Exception>().WithMessage("Disposing");

            control.Release.Set();
            shell.ProcessExited += Raise.WithEmpty();
            await disposingTask;
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenDisposed()
        {
            // arrange

            // ExifToolArguments.StayOpen, ExifToolArguments.BoolFalse
            A.CallTo(() => shell.WriteLineAsync(ExifToolArguments.BoolFalse))
                .ReturnsLazily(async call =>
                {
                    await Task.Yield();
                    shell.ProcessExited += Raise.WithEmpty();
                });

            sut.Initialize();
            await sut.DisposeAsync();

            // act
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null);

            // assert
            act.Should().Throw<Exception>().WithMessage("Disposed");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPassArgsToInnerShellAndFinishWihExecute()
        {
            // arrange
            var args = new[] { "abc", "def" };
            sut.Initialize();

            // act
            _ = await sut.ExecuteAsync(args, CancellationToken.None);

            // assert
            A.CallTo(() => shell.WriteLineAsync("abc")).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => shell.WriteLineAsync("def")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => shell.WriteLineAsync("-execute1")).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnWithStreamResult()
        {
            // arrange
            var args = new[] { "abc", "def" };
            sut.Initialize();

            // act
            var result = await sut.ExecuteAsync(args, CancellationToken.None);

            // assert
            result.Should().Be($"fake result abc{OperatingSystemHelper.NewLine}fake result def");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFinishAllRequestsAlthoughOneWasCancelled()
        {
            // arrange
            var controlExecute1 = testSut.SetupControl("-execute1");
            var cts = new CancellationTokenSource();
            sut.Initialize();

            // act
            var resultTask1 = sut.ExecuteAsync(new[] { "a" }, CancellationToken.None);
            controlExecute1.Entered.WaitOne(); // make sure resultTask1 is executing and has 'entered' ExifTool
            var resultTask2 = sut.ExecuteAsync(new[] { "b" }, cts.Token);
            var resultTask3 = sut.ExecuteAsync(new[] { "c" }, CancellationToken.None);
            cts.Cancel(); // cancel second task
            controlExecute1.Release.Set(); // signal 'exiftool' to finish the request of task1.

            var result1 = await resultTask1;
            await IgnoreException(resultTask2);
            var result3 = await resultTask3;

            // assert
            result1.Should().Be("fake result a");
            resultTask2.IsCanceled.Should().BeTrue();
            result3.Should().Be("fake result c");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPassArgumentsToExiftoolUnlessCommandWasCancelled()
        {
            // arrange
            var controlExecute1 = testSut.SetupControl("-execute1");
            var cts = new CancellationTokenSource();
            sut.Initialize();

            // act
            var resultTask1 = sut.ExecuteAsync(new[] { "a" }, CancellationToken.None);
            controlExecute1.Entered.WaitOne(); // make sure resultTask1 is executing and has 'entered' ExifTool
            var resultTask2 = sut.ExecuteAsync(new[] { "b" }, cts.Token);
            var resultTask3 = sut.ExecuteAsync(new[] { "c" }, CancellationToken.None);
            cts.Cancel(); // cancel second task
            controlExecute1.Release.Set(); // signal 'exiftool' to finish the request of task1.

            await resultTask1;
            await IgnoreException(resultTask2);
            await resultTask3;

            // assert
            A.CallTo(() => shell.WriteLineAsync("a")).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => shell.WriteLineAsync("-execute1")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => shell.WriteLineAsync("c")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => shell.WriteLineAsync("-execute2")).MustHaveHappenedOnceExactly());

            A.CallTo(() => shell.WriteLineAsync("b")).MustNotHaveHappened();
            A.CallTo(() => shell.WriteLineAsync("-execute3")).MustNotHaveHappened();
        }

        private async Task IgnoreException(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private class TestableAsyncFakeExifTool : AsyncExifTool
        {
            private readonly IShell shell;
            private readonly ConcurrentDictionary<string, ExifToolCommandControlToken> exiftoolControl;
            private Stream exifToolStream;

            public TestableAsyncFakeExifTool(IShell shell)
                : base(AsyncExifToolConfigurationFactory.Create())
            {
                this.shell = shell;
                exiftoolControl = new ConcurrentDictionary<string, ExifToolCommandControlToken>();
                CreateShellCalled = 0;
            }

            public int CreateShellCalled { get; private set; }

            public ExifToolCommandControlToken SetupControl(string key)
            {
                var control = new ExifToolCommandControlToken();
                exiftoolControl.TryAdd(key, control);
                return control;
            }

            public ExifToolCommandControlToken SetupCreateShellControl()
            {
                var control = new ExifToolCommandControlToken();
                exiftoolControl.TryAdd(nameof(CreateShell), control);
                return control;
            }

            internal override IShell CreateShell(
                string exifToolFullPath,
                IEnumerable<string> args,
                Stream outputStream,
                Stream errorStream)
            {
                exifToolStream = Stream.Synchronized(outputStream);

                CreateShellCalled++;
                if (exiftoolControl.TryGetValue(nameof(CreateShell), out var control))
                {
                    control.Entered.Set();
                    control.Release.WaitOne(TimeSpan.FromMinutes(1));
                }

                A.CallTo(() => shell.WriteLineAsync(A<string>._))
                    .Invokes(async call =>
                    {
                        if (!(call.Arguments[0] is string text))
                            throw new ArgumentNullException("text");

                        if (exiftoolControl.TryGetValue(text, out var control))
                        {
                            control.Entered.Set();
                            await control.Release.AsTask();
                        }

                        if (text.StartsWith("-execute"))
                        {
                            var result = text.Replace("-execute", "{ready") + "}";

                            var data = Encoding.UTF8.GetBytes(result + OperatingSystemHelper.NewLine);
#if NETCOREAPP3_0
                            await exifToolStream.WriteAsync(data);
#else
                            await exifToolStream.WriteAsync(data, 0, data.Length);
#endif
                            return;
                        }

                        var dataFakeResult = Encoding.UTF8.GetBytes($"fake result {text}{OperatingSystemHelper.NewLine}");
#if NETCOREAPP3_0
                        await exifToolStream.WriteAsync(dataFakeResult);
#else
                        await exifToolStream.WriteAsync(dataFakeResult, 0, dataFakeResult.Length);
#endif

                    });

                return shell;
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
