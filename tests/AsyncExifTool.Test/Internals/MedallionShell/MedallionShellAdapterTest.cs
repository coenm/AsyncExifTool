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
    using TestHelper.XUnit.Facts;
    using Xunit;
    using Xunit.Abstractions;

    [Xunit.Categories.IntegrationTest]
    public class MedallionShellAdapterTest : IAsyncLifetime
    {
        private const int FALLBACK_TEST_TIMEOUT = 5000;
        private readonly ITestOutputHelper _output;
        private readonly Stream _stream;
        private readonly ManualResetEventSlim _mreSutExited;
        private readonly MedallionShellAdapter _sut;

        public MedallionShellAdapterTest(ITestOutputHelper output)
        {
            _output = output;
            _mreSutExited = new ManualResetEventSlim(false);
            var defaultArgs = new List<string>
                                    {
                                        ExifToolArguments.STAY_OPEN,
                                        ExifToolArguments.BOOL_TRUE,
                                        "-@",
                                        "-",
                                    };

            _stream = new WriteDelegatedDummyStream(new ExifToolStdOutWriter(Encoding.UTF8));
            var errorStream = new WriteDelegatedDummyStream(new ExifToolStdErrWriter(Encoding.UTF8));

            _sut = new MedallionShellAdapter(ExifToolSystemConfiguration.ExifToolExecutable, defaultArgs, _stream, errorStream);
            _sut.ProcessExited += SutOnProcessExited;
            _sut.Initialize();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _sut.ProcessExited -= SutOnProcessExited;
            await _sut.TryCancelAsync();
            _sut.Kill();
            _sut.Dispose();
            _stream.Dispose();
        }

        [ExifTool]
        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyorWindows, reason: "Sometimes this tests hangs on AppVeyor (windows).")]
        public async Task KillingSut_ShouldInvokeProcessExitedEventTest()
        {
            // arrange

            // assume
            _sut.Finished.Should().BeFalse();

            // act
            _sut.Kill();
            await _sut.Task.ConfigureAwait(false);

            // assert
            AssertSutFinished(FALLBACK_TEST_TIMEOUT);
        }

        [Fact]
        [ExifTool]
        public async Task SettingStayOpenToFalseShouldCloseSutTest()
        {
            // arrange

            // assume
            _sut.Finished.Should().BeFalse();

            // act
            await _sut.WriteLineAsync(ExifToolArguments.STAY_OPEN).ConfigureAwait(false);
            await _sut.WriteLineAsync(ExifToolArguments.BOOL_FALSE).ConfigureAwait(false);

            _output.WriteLine("Awaiting task to finish");
            await _sut.Task.ConfigureAwait(false);
            _output.WriteLine("Task finished");

            // assert
            AssertSutFinished(FALLBACK_TEST_TIMEOUT);
        }

        private void AssertSutFinished(int timeout = 0)
        {
            _mreSutExited.Wait(timeout);
            _mreSutExited.IsSet.Should().BeTrue($"{nameof(_sut.ProcessExited)} event should have been fired.");
            _sut.Task.IsCompleted.Should().BeTrue("Task should have been completed.");
            _sut.Finished.Should().BeTrue($"{nameof(_sut.Finished)} property should be true.");
        }

        private void SutOnProcessExited(object sender, EventArgs eventArgs)
        {
            _mreSutExited.Set();
        }
    }
}
