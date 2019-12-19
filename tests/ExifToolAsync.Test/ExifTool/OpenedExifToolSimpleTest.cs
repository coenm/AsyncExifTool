namespace ExifToolAsyncTest.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using ExifToolAsync.ExifTool;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class OpenedExifToolSimpleTest
    {
        private readonly OpenedExifTool sut;
        private readonly IMedallionShell medallionShell;
        private readonly List<string> calledArguments;

        public OpenedExifToolSimpleTest()
        {
            calledArguments = new List<string>();
            medallionShell = A.Fake<IMedallionShell>();
            sut = new TestableOpenedExifTool(medallionShell);
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

/*
        [Fact]
        public async Task ExecuteAsyncWithoutInitializingShouldThrowTestaaaa()
        {
            // arrange

            const int TEST_TIMEOUT = 1000;
            var mre = new ManualResetEventSlim(false);
            var mre2 = new ManualResetEventSlim(false);
            A.CallTo(() => mediallionShell.WriteLineAsync(A<string>._))
             .Invokes(call => calledArguments.Add((string)call.Arguments[0]))
             .ReturnsLazily(async call =>
                            {
                                var line = (string)call.Arguments[0];
                                if (line.StartsWith("-execute"))
                                {
                                    await Task.Yield();
                                    mre2.Set();
                                    if (!mre.Wait(TEST_TIMEOUT))
                                        throw new TimeoutException();
                                }
                            });

            sut.Init();

            // act
            var resultTask = _sut.ExecuteAsync("arg 1").ConfigureAwait(false);

            mre2.Wait(TEST_TIMEOUT);
            mre.Set();


            // assert
            act.Should().Throw<Exception>().WithMessage("Not initialized");
        }
*/

        private class TestableOpenedExifTool : OpenedExifTool
        {
            private readonly IMedallionShell medallionShell;

            public TestableOpenedExifTool(IMedallionShell medallionShell)
                : base("doesn't matter")
            {
                this.medallionShell = medallionShell;
            }

            protected override IMedallionShell CreateExitToolMedallionShell(string exifToolPath, List<string> defaultArgs, Stream outputStream, Stream errorStream)
            {
                return medallionShell;
            }
        }
    }
}
