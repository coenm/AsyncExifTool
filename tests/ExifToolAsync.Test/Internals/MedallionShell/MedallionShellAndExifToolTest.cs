namespace ExifToolAsyncTest.Internals.MedallionShell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EagleEye.TestHelper.XUnit;
    using ExifToolAsync.Internals;
    using ExifToolAsync.Internals.Stream;
    using ExifToolAsyncTest.TestInternals;
    using FluentAssertions;
    using Medallion.Shell;
    using TestHelper;
    using TestHelper.XUnit.Facts;
    using Xunit;
    using Xunit.Abstractions;

    public class MedallionShellAndExifToolTest
    {
        private readonly string image;
        private readonly ITestOutputHelper output;
        private readonly string currentExifToolVersion;

        // These tests will only run when exiftool is available from PATH.
        public MedallionShellAndExifToolTest(ITestOutputHelper output)
        {
            currentExifToolVersion = ExifToolSystemConfiguration.ConfiguredVersion;
            this.output = output;

            image = Directory
                .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                .SingleOrDefault();

            this.output.WriteLine($"Testfile: {image}");

            var exists = File.Exists(image);
            exists.Should().BeTrue("File does NOT! exists!!");

            image.Should().NotBeNullOrEmpty();
        }

        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyorWindows)]
        [ExifTool]
        public async Task RunExifToolWithCustomStream()
        {
            // arrange
            IEnumerable<string> args = new List<string>
            {
                ExifToolArguments.StayOpen,
                ExifToolArguments.BoolTrue,
                "-@",
                "-",
                "-common_args",
                ExifToolArguments.JsonOutput,

                // format coordinates as signed decimals.
                "-c",
                "%+.6f",

                "-struct",
                "-g", // group
            };

            var capturedExifToolResults = new Dictionary<string, string>();

            void StreamOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
            {
                capturedExifToolResults.Add(dataCapturedArgs.Key, dataCapturedArgs.Data);
            }

            async Task DelayX()
            {
                await Task.Delay(10);
            }

            await using var stream = new ExifToolStayOpenStream(new UTF8Encoding());
            stream.Update += StreamOnUpdate;

            // act
            var cmd = Command.Run(ExifToolSystemConfiguration.ExifToolExecutable, args).RedirectTo(stream);

            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync(ExifToolArguments.Version).ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-execute0000").ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync(image).ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-execute0005").ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync(image).ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-execute0008").ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-stay_open").ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("False").ConfigureAwait(false);
            await DelayX().ConfigureAwait(false);

            ProtectAgainstHangingTask(cmd);
            var result = await cmd.Task.ConfigureAwait(false);

            await DelayX().ConfigureAwait(false);
            stream.Update -= StreamOnUpdate;

            // assert
            result.StandardError.Should().BeNullOrEmpty("No errors expected.");
            result.Success.Should().BeTrue("success should be true.");
            capturedExifToolResults.Should().HaveCount(3).And.ContainKeys("0000", "0005", "0008");

            output.WriteLine("results:");
            foreach (var item in capturedExifToolResults)
                output.WriteLine($"- {item.Key} : {item.Value}");
        }

        private static void ProtectAgainstHangingTask(Command cmd)
        {
            if (cmd.Task.Wait(TimeSpan.FromSeconds(12)))
                return;

            cmd.Kill();
            throw new Exception("Could not close Exiftool without killing it.");
        }

        private void WriteResultToOutput(CommandResult result)
        {
            if (result == null)
            {
                output.WriteLine("result is null");
                return;
            }

            output.WriteLine($"ExitCode: {result.ExitCode}");
            output.WriteLine($"Success: {result.Success}");
            output.WriteLine($"StandardOutput: {result.StandardOutput}");
            output.WriteLine($"StandardError: {result.StandardError}");
        }
    }
}
