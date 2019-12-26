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
        public void ExecuteAsyncWithoutInitializingShouldThrowTest()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null).ConfigureAwait(false);

            // assert
            act.Should().Throw<Exception>().WithMessage("Not initialized");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPassArgsToInnerShell()
        {
            // arrange
            var args = new[] { "abc", "def" };
            var ct = new CancellationTokenSource().Token;
            A.CallTo(() => shell.WriteLineAsync("-execute1"))
                .Invokes(call =>
                    {
                        fakeExifTool.ExifToolStream.Write(Encoding.UTF8.GetBytes("fake result\r\n"));
                        fakeExifTool.ExifToolStream.Write(Encoding.UTF8.GetBytes("{ready1}\r\n"));
                    });

            sut.Init();

            // act
            var result = await sut.ExecuteAsync(args, ct);

            // assert
            A.CallTo(() => shell.WriteLineAsync("abc")).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => shell.WriteLineAsync("def")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => shell.WriteLineAsync("-execute1")).MustHaveHappenedOnceExactly());
        }

        private interface IExifToolOutput
        {
            Stream ExifToolStream { get; }
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
        }
    }
}
