namespace CoenM.ExifToolLibTest.Integration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib;
    using CoenM.ExifToolLibTest.TestInternals;
    using EagleEye.TestHelper.XUnit;
    using FluentAssertions;
    using TestHelper;
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
        }

        public void Dispose()
        {
            var originalSuffixedFiles = Directory.GetFiles(TestImages.InputImagesDirectoryFullPath, "*.jpg_original", SearchOption.AllDirectories);

            foreach (var originalSuffixFile in originalSuffixedFiles)
            {
                var originalFilename = originalSuffixFile.Replace("_original", string.Empty);

                if (File.Exists(originalFilename))
                    File.Delete(originalFilename);

                File.Move(originalSuffixFile, originalFilename);
            }
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task WriteXmpSubjectsToImageTest()
        {
            // arrange
            await using var sut = new AsyncExifTool(ExifToolSystemConfiguration.ExifToolExecutable);
            sut.Initialize();

            // act
            var @params = new[]
            {
                "-XMP-dc:Subject+=def",
                "-XMP-dc:Subject+=abc",
                "-XMP-dc:Subject=xyz",
                image,
            };

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
        }
    }
}
