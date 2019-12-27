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
        private readonly IExifToolOutput fakeExifTool;
        private readonly IShell shell;
        private readonly List<string> calledArguments;

        public OpenedExifToolSimpleTest()
        {
            calledArguments = new List<string>();
            shell = A.Fake<IShell>();
            sut = new TestableOpenedExifTool(shell);
            fakeExifTool = sut as IExifToolOutput;

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
            var mre = new ManualResetEvent(false);
            var mre2 = new ManualResetEvent(false);

            // ExifToolArguments.StayOpen, ExifToolArguments.BoolFalse 
            A.CallTo(() => shell.WriteLineAsync(ExifToolArguments.BoolFalse))
                .ReturnsLazily(async call =>
                {
                    mre.Set();
                    await Task.Yield();
                    mre2.WaitOne();
                    shell.ProcessExited += Raise.WithEmpty();
                });

            sut.Init();
            var disposingTask = sut.DisposeAsync();

            // act
            mre.WaitOne();
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null);

            // assert
            act.Should().Throw<Exception>().WithMessage("Disposing");

            mre2.Set();
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
        public async Task ExecuteAsync_ShouldPassArgsToInnerShellAndFinishWihExecute1()
        {
            // arrange
            var args = new[] { "abc", "def" };
            var ct = new CancellationTokenSource().Token;
            A.CallTo(() => shell.WriteLineAsync("-execute1"))
                .Invokes(call =>
                    {
                        fakeExifTool.WriteToStream(Encoding.UTF8.GetBytes("fake result" + OperatingSystemHelper.NewLine));
                        fakeExifTool.WriteToStream(Encoding.UTF8.GetBytes("{ready1}" + OperatingSystemHelper.NewLine));
                    });

            sut.Init();

            // act
            var result = await sut.ExecuteAsync(args, ct);

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
            var ct = new CancellationTokenSource().Token;
            A.CallTo(() => shell.WriteLineAsync("-execute1"))
                .Invokes(call =>
                    {
                        fakeExifTool.WriteToStream(Encoding.UTF8.GetBytes("fake result" + OperatingSystemHelper.NewLine));
                        fakeExifTool.WriteToStream(Encoding.UTF8.GetBytes("{ready1}" + OperatingSystemHelper.NewLine));
                    });

            sut.Init();

            // act
            var result = await sut.ExecuteAsync(args, ct);

            // assert
            result.Should().Be("fake result");
        }

        private interface IExifToolOutput
        {
            Stream ExifToolStream { get; }

            void WriteToStream(Span<byte> data);
        }

        private class TestableOpenedExifTool : OpenedExifTool, IExifToolOutput
        {
            private readonly IShell shell;

            public TestableOpenedExifTool(IShell shell)
                : base("doesn't matter")
            {
                this.shell = shell;
            }

            public Stream ExifToolStream { get; private set; }

            internal override IShell CreateShell(string exifToolPath, IEnumerable<string> defaultArgs, Stream outputStream, Stream errorStream)
            {
                ExifToolStream = outputStream;
                return shell;
            }

            void IExifToolOutput.WriteToStream(Span<byte> data)
            {
                ExifToolStream.Write(data);
            }
        }
    }
}
