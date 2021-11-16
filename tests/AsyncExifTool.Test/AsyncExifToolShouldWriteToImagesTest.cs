namespace CoenM.ExifToolLibTest
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
    using TestHelper.XUnit.Facts;
    using Xunit;
    using Xunit.Abstractions;

    public class AsyncExifToolShouldWriteToImagesTest : IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;
        private string _image;

        public AsyncExifToolShouldWriteToImagesTest(ITestOutputHelper output)
        {
            _output = output;

            _image = Directory
                     .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            _image.Should().NotBeNullOrEmpty("Image should exist on system.");
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
            {
                Directory.CreateDirectory(tmpPath);
            }

            Directory.Exists(tmpPath).Should().BeTrue("Temp path should exists");

            var newImagePath = Path.Combine(tmpPath, nameof(AsyncExifToolShouldWriteToImagesTest) + new FileInfo(_image).Name);
            if (!File.Exists(newImagePath))
            {
                File.Copy(_image, newImagePath);
            }

            File.Exists(newImagePath).Should().BeTrue("Image should have been copied to temp directory.");

            _image = newImagePath;
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            if (TestEnvironment.RunsOnDevOps && TestEnvironment.IsWindows)
            {
                return Task.CompletedTask;
            }

            if (File.Exists(_image))
            {
                File.Delete(_image);
            }

            if (File.Exists(_image + "_original"))
            {
                File.Delete(_image + "_original");
            }

            return Task.CompletedTask;
        }

        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [Fact]
        public async Task WriteXmpSubjectsToImageTest()
        {
            // arrange
#if NETCOREAPP3_1
            await using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#else
            using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#endif
            sut.Initialize();

            // act
            var @params = new[] { "-XMP-dc:Subject+=def", "-XMP-dc:Subject+=abc", "-XMP-dc:Subject=xyz", _image, };

            var readResultBefore = await sut.ExecuteAsync(_image).ConfigureAwait(false);
            var writeResult = await sut.ExecuteAsync(@params).ConfigureAwait(false);
            var readResultAfter = await sut.ExecuteAsync(_image).ConfigureAwait(false);

            // assert
            readResultBefore.Should().Contain("Subject                         : dog, new york, puppy");
            readResultBefore.Should().NotContain("Subject                         : dog, new york, puppy, def, abc, xyz");
            writeResult.Should().Be("    1 image files updated" + Environment.NewLine);
            readResultAfter.Should().Contain("Subject                         : dog, new york, puppy, def, abc, xyz" + Environment.NewLine);

            // just for fun
            _output.WriteLine(readResultAfter);
        }
    }
}
