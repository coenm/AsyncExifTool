namespace ExifToolAsyncTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using ExifToolAsync;
    using ExifToolAsync.Internals;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class OpenedExifToolSimpleTest
    {
        private readonly OpenedExifTool sut;
        private readonly IShell shell;
        private readonly List<string> calledArguments;

        public OpenedExifToolSimpleTest()
        {
            calledArguments = new List<string>();
            shell = A.Fake<IShell>();
            sut = new TestableOpenedExifTool(shell);
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

        private class TestableOpenedExifTool : OpenedExifTool
        {
            private readonly IShell shell;

            public TestableOpenedExifTool(IShell shell)
                : base("doesn't matter")
            {
                this.shell = shell;
            }

            internal override IShell CreateShell(string exifToolPath, IEnumerable<string> defaultArgs, Stream outputStream, Stream errorStream)
            {
                return shell;
            }
        }
    }
}
