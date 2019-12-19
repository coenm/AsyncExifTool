namespace ExifToolAsyncTest.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EagleEye.TestHelper.XUnit;
    using ExifToolAsync;
    using ExifToolAsync.ExifTool;
    using ExifToolAsync.Internals;
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

        [Fact]
        [ExifTool]
        public async Task RunExifToolToGetVersion()
        {
            // arrange
            var args = new List<string>
            {
                ExifToolArguments.Version,
            };

            // act
            var cmd = Command.Run(ExifToolSystemConfiguration.ExifToolExecutable, args);
            ProtectAgainstHangingTask(cmd);
            var result = await cmd.Task.ConfigureAwait(false);

            // assert
            WriteResultToOutput(result);
            output.WriteLine($"Received exiftool version: {cmd.Result.StandardOutput}");
            cmd.Result.StandardOutput.Should().Be($"{currentExifToolVersion}\r\n".ConvertToOsString());
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

            using var stream = new ExifToolStayOpenStream(new UTF8Encoding());
            stream.Update += StreamOnUpdate;

            // act
            var cmd = Command.Run(ExifToolSystemConfiguration.ExifToolExecutable, args).RedirectTo(stream);

            await cmd.StandardInput.WriteLineAsync(ExifToolArguments.Version).ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-execute0000").ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync(image).ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-execute0005").ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync(image).ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-execute0008").ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("-stay_open").ConfigureAwait(false);
            await cmd.StandardInput.WriteLineAsync("False").ConfigureAwait(false);

            ProtectAgainstHangingTask(cmd);
            var result = await cmd.Task.ConfigureAwait(false);

            stream.Update -= StreamOnUpdate;

            // assert
            cmd.Result.Success.Should().BeTrue();
            cmd.Result.StandardError.Should().BeNullOrEmpty();
            capturedExifToolResults.Should().HaveCount(3).And.ContainKeys("0000", "0005", "0008");
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
