namespace ExifToolAsyncTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ExifToolAsync;
    using ExifToolAsync.Internals;
    using FakeItEasy;
    using FluentAssertions;
    using TestHelper;
    using Xunit;

    public class OpenedExifToolSimpleTest
    {
        private readonly OpenedExifTool sut;
        private readonly IFakeExifTool fakeFakeExifTool;
        private readonly IShell shell;
        private readonly List<string> calledArguments;

        public OpenedExifToolSimpleTest()
        {
            calledArguments = new List<string>();
            shell = A.Fake<IShell>();
            sut = new TestableOpenedFakeExifTool(shell);
            fakeFakeExifTool = sut as IFakeExifTool;

            A.CallTo(() => shell.WriteLineAsync(A<string>._)).Returns(Task.CompletedTask);
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
            var control = fakeFakeExifTool.SetupControl(ExifToolArguments.BoolFalse);
            sut.Init();
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

            sut.Init();
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
            sut.Init();

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
            sut.Init();

            // act
            var result = await sut.ExecuteAsync(args, CancellationToken.None);

            // assert
            result.Should().Be($"fake result abc{OperatingSystemHelper.NewLine}fake result def");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFinishAllRequestsAlthoughOneWasCancelled()
        {
            // arrange
            var controlExecute1 = fakeFakeExifTool.SetupControl("-execute1");
            var cts = new CancellationTokenSource();
            sut.Init();

            // act
            var resultTask1 = sut.ExecuteAsync(new[] { "a" }, CancellationToken.None);
            controlExecute1.Entered.WaitOne(); // make sure resultTask1 is executing and has 'entered' ExifTool
            var resultTask2 = sut.ExecuteAsync(new[] { "b" }, cts.Token);
            var resultTask3 = sut.ExecuteAsync(new[] { "c" }, CancellationToken.None);
            cts.Cancel(); // cancel second task
            controlExecute1.Release.Set(); // signal 'exiftool' to finish the request of task1.

            // assert
            (await resultTask1).Should().Be("fake result a");
            resultTask2.IsCanceled.Should().BeTrue();
            (await resultTask3).Should().Be("fake result c");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPassArgumentsToExiftoolUnlessCommandWasCancelled()
        {
            // arrange
            var controlExecute1 = fakeFakeExifTool.SetupControl("-execute1");
            var cts = new CancellationTokenSource();
            sut.Init();

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

        private interface IFakeExifTool
        {
            ExifToolCommandControl SetupControl(string key);
        }

        private class TestableOpenedFakeExifTool : OpenedExifTool, IFakeExifTool
        {
            private readonly IShell shell;
            private readonly Dictionary<string, ExifToolCommandControl> exiftoolControl;
            private Stream exifToolStream;

            public TestableOpenedFakeExifTool(IShell shell)
                : base("doesn't matter")
            {
                this.shell = shell;
                exiftoolControl = new Dictionary<string, ExifToolCommandControl>();
            }

            internal override IShell CreateShell(string exifToolPath, IEnumerable<string> defaultArgs, Stream outputStream, Stream errorStream)
            {
                exifToolStream = Stream.Synchronized(outputStream);

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
                            await exifToolStream.WriteAsync(Encoding.UTF8.GetBytes(result + OperatingSystemHelper.NewLine));
                            return;
                        }

                        await exifToolStream.WriteAsync(Encoding.UTF8.GetBytes($"fake result {text}{OperatingSystemHelper.NewLine}"));
                    });

                return shell;
            }

            ExifToolCommandControl IFakeExifTool.SetupControl(string key)
            {
                var control = new ExifToolCommandControl();
                exiftoolControl.TryAdd(key, control);
                return control;
            }
        }
    }

    internal class ExifToolCommandControl
    {
        public ExifToolCommandControl()
        {
            Entered = new ManualResetEvent(false);
            Release = new ManualResetEvent(false);
        }

        public ManualResetEvent Entered { get; }

        public ManualResetEvent Release { get; }
    }
}
