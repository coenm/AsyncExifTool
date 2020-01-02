namespace CoenM.ExifToolLibTest.Integration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib;
    using CoenM.ExifToolLibTest.TestInternals;
    using EagleEye.TestHelper.XUnit;
    using FluentAssertions;
    using TestHelper;
    using TestHelper.XUnit.Facts;
    using Xunit;
    using Xunit.Abstractions;

    public class AsyncExifToolShouldWriteToImagesTest : IDisposable
    {
        private readonly string image;
        private readonly ITestOutputHelper output;

        public AsyncExifToolShouldWriteToImagesTest(ITestOutputHelper output)
        {
            this.output = output;

            image = Directory
                     .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            image.Should().NotBeNullOrEmpty("Image should exist on system.");

            string tmpPath = Path.GetTempPath();
            tmpPath.Should().NotBeNullOrWhiteSpace("We need an temp path");
            if (!Directory.Exists(tmpPath))
                Directory.CreateDirectory(tmpPath);

            Directory.Exists(tmpPath).Should().BeTrue("Temp path should exists");

            var newImagePath = Path.Combine(tmpPath, new FileInfo(image).Name);
            if (!File.Exists(newImagePath))
                File.Copy(image, newImagePath);

            File.Exists(newImagePath).Should().BeTrue("Ìmage should have been copied to temp directory.");

            image = newImagePath;
        }

        public void Dispose()
        {
            if (File.Exists(image))
                File.Delete(image);
            if (File.Exists(image + "_original"))
                File.Delete(image + "_original");
        }

        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [ConditionalHostFact(TestHostMode.Skip, TestHost.AzureDevopsWindows, reason: "This test is probably the reason that DevOps agent running on windows hangs.")]
        public async Task WriteXmpSubjectsToImageTest()
        {
            // arrange
            await using (var sut = new AsyncExifTool(ExifToolSystemConfiguration.ExifToolExecutable))
            {
                sut.Initialize();

                // act
                var @params = new[] {"-XMP-dc:Subject+=def", "-XMP-dc:Subject+=abc", "-XMP-dc:Subject=xyz", image,};

                var readResultBefore = await sut.ExecuteAsync(image).ConfigureAwait(false);
                var writeResult = await sut.ExecuteAsync(@params).ConfigureAwait(false);
                var readResultAfter = await sut.ExecuteAsync(image).ConfigureAwait(false);

                // assert
                readResultBefore.Should().Contain("Subject                         : dog, new york, puppy");
                readResultBefore.Should().NotContain("Subject                         : dog, new york, puppy, def, abc, xyz");
                writeResult.Should().Be("    1 image files updated");
                readResultAfter.Should().Contain("Subject                         : dog, new york, puppy, def, abc, xyz");

                // just for fun
                output.WriteLine(readResultAfter);

                await sut.DisposeAsync(new CancellationTokenSource(2000).Token);
            }
        }
    }
}
