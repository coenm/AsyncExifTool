namespace CoenM.ExifToolLibTest.Internals.MedallionShell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals;
    using CoenM.ExifToolLib.Internals.MedallionShell;
    using CoenM.ExifToolLib.Internals.Stream;
    using CoenM.ExifToolLibTest.TestInternals;
    using EagleEye.TestHelper.XUnit;
    using FluentAssertions;
    using TestHelper;
    using TestHelper.XUnit.Facts;
    using Xunit;
    using Xunit.Abstractions;

    [Xunit.Categories.IntegrationTest]
    public class MedallionShellAdapterTest : IAsyncLifetime
    {
        private const int FallbackTestTimeout = 5000;
        private readonly ITestOutputHelper output;
        private readonly Stream stream;
        private readonly ManualResetEventSlim mreSutExited;
        private readonly MedallionShellAdapter sut;

        public MedallionShellAdapterTest(ITestOutputHelper output)
        {
            this.output = output;
            mreSutExited = new ManualResetEventSlim(false);
            var defaultArgs = new List<string>
                                    {
                                        ExifToolArguments.StayOpen,
                                        ExifToolArguments.BoolTrue,
                                        "-@",
                                        "-",
                                    };

            stream = new WriteDelegatedDummyStream(new ExifToolStdOutWriter(Encoding.UTF8, OperatingSystemHelper.NewLine));
            var errorStream = new WriteDelegatedDummyStream(new ExifToolStdErrWriter(Encoding.UTF8));

            sut = new MedallionShellAdapter(ExifToolSystemConfiguration.ExifToolExecutable, defaultArgs, stream, errorStream);
            sut.ProcessExited += SutOnProcessExited;
            sut.Initialize();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            sut.ProcessExited -= SutOnProcessExited;
            await sut.TryCancelAsync();
            sut.Kill();
            sut.Dispose();
            stream.Dispose();
        }

        [ExifTool]
        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyorWindows, reason: "Sometimes this tests hangs on AppVeyor (windows).")]
        public async Task KillingSut_ShouldInvokeProcessExitedEventTest()
        {
            // arrange

            // assume
            sut.Finished.Should().BeFalse();

            // act
            sut.Kill();
            await sut.Task.ConfigureAwait(false);

            // assert
            AssertSutFinished(FallbackTestTimeout);
        }

        [Fact]
        [ExifTool]
        public async Task SettingStayOpenToFalseShouldCloseSutTest()
        {
            // arrange

            // assume
            sut.Finished.Should().BeFalse();

            // act
            await sut.WriteLineAsync(ExifToolArguments.StayOpen).ConfigureAwait(false);
            await sut.WriteLineAsync(ExifToolArguments.BoolFalse).ConfigureAwait(false);

            output.WriteLine("Awaiting task to finish");
            await sut.Task.ConfigureAwait(false);
            output.WriteLine("Task finished");

            // assert
            AssertSutFinished(FallbackTestTimeout);
        }

        private void AssertSutFinished(int timeout = 0)
        {
            mreSutExited.Wait(timeout);
            mreSutExited.IsSet.Should().BeTrue($"{nameof(sut.ProcessExited)} event should have been fired.");
            sut.Task.IsCompleted.Should().BeTrue("Task should have been completed.");
            sut.Finished.Should().BeTrue($"{nameof(sut.Finished)} property should be true.");
        }

        private void SutOnProcessExited(object sender, EventArgs eventArgs)
        {
            mreSutExited.Set();
        }
    }
}
