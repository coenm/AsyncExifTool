namespace CoenM.ExifToolLibTest
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib;
    using CoenM.ExifToolLibTest.TestInternals;
    using EagleEye.TestHelper.XUnit;
    using FluentAssertions;
    using TestHelper;
    using TestHelper.XUnit.Facts;
    using Xunit;
    using Xunit.Abstractions;

    public class AsyncExifToolShouldWriteToImagesTest : IAsyncLifetime
    {
        private readonly ITestOutputHelper output;
        private string image;

        public AsyncExifToolShouldWriteToImagesTest(ITestOutputHelper output)
        {
            this.output = output;

            image = Directory
                     .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            image.Should().NotBeNullOrEmpty("Image should exist on system.");
        }

        public Task InitializeAsync()
        {
            if (TestEnvironment.RunsOnDevOps && TestEnvironment.IsWindows)
            {
                return Task.CompletedTask;
            }

            var tmpPath = Path.GetTempPath();
            tmpPath.Should().NotBeNullOrWhiteSpace("We need an temp path");
            if (!Directory.Exists(tmpPath))
                Directory.CreateDirectory(tmpPath);

            Directory.Exists(tmpPath).Should().BeTrue("Temp path should exists");

            var newImagePath = Path.Combine(tmpPath, new FileInfo(image).Name);
            if (!File.Exists(newImagePath))
                File.Copy(image, newImagePath);

            File.Exists(newImagePath).Should().BeTrue("Image should have been copied to temp directory.");

            image = newImagePath;
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            if (TestEnvironment.RunsOnDevOps && TestEnvironment.IsWindows)
                return Task.CompletedTask;

            if (File.Exists(image))
                File.Delete(image);
            if (File.Exists(image + "_original"))
                File.Delete(image + "_original");

            return Task.CompletedTask;
        }

        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [ConditionalHostFact(TestHostMode.Skip, TestHost.AzureDevopsWindows, reason: "This test is probably the reason that DevOps agent running on windows hangs.")]
        public async Task WriteXmpSubjectsToImageTest()
        {
            // arrange
            await using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            sut.Initialize();

            // act
            var @params = new[] { "-XMP-dc:Subject+=def", "-XMP-dc:Subject+=abc", "-XMP-dc:Subject=xyz", image, };

            var readResultBefore = await sut.ExecuteAsync(image).ConfigureAwait(false);
            var writeResult = await sut.ExecuteAsync(@params).ConfigureAwait(false);
            var readResultAfter = await sut.ExecuteAsync(image).ConfigureAwait(false);

            // assert
            readResultBefore.Should().Contain("Subject                         : dog, new york, puppy");
            readResultBefore.Should()
                .NotContain("Subject                         : dog, new york, puppy, def, abc, xyz");
            writeResult.Should().Be("    1 image files updated");
            readResultAfter.Should().Contain("Subject                         : dog, new york, puppy, def, abc, xyz");

            // just for fun
            output.WriteLine(readResultAfter);
        }
    }
}
