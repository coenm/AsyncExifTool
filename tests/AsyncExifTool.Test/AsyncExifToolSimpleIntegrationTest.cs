namespace CoenM.ExifToolLibTest
{
    using System;
    using System.Diagnostics;
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

    public class AsyncExifToolSimpleIntegrationTest
    {
        private const int REPEAT = 100;
        private readonly string _image;
        private readonly ITestOutputHelper _output;

        public AsyncExifToolSimpleIntegrationTest(ITestOutputHelper output)
        {
            this._output = output;

            _image = Directory
                     .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            _image.Should().NotBeNullOrEmpty("Image should exist on system.");
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task RunExiftoolForVersionAndImageTest()
        {
            // arrange
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            sut.Initialize();

            // act
            var version = await sut.GetVersionAsync().ConfigureAwait(false);
            var result = await sut.ExecuteAsync(_image).ConfigureAwait(false);

            // assert
            version.Should().NotBeNullOrEmpty();
            result.Should().NotBeNullOrEmpty();

            await sut.DisposeAsync().ConfigureAwait(false);

            // just for fun
            _output.WriteLine(version);
            _output.WriteLine(result);
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task RunExiftoolForVersionAndImageErrorTest()
        {
            // arrange
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            sut.Initialize();

            // act
            Func<Task> act = async () => await sut.ExecuteAsync(_image + "does not exist");
            var version = await sut.GetVersionAsync().ConfigureAwait(false);

            // assert
            act.Should().Throw<Exception>();
            version.Should().NotBeNullOrEmpty();

            await sut.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task RunExiftoolGetImageSizeAndExposureTime()
        {
            // arrange
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            sut.Initialize();

            // act
            var result = await sut.ExecuteAsync(
                    new[]
                    {
                        "-s",
                        "-ImageSize",
                        "-ExposureTime",
                        _image,
                    })
                .ConfigureAwait(false);

            // assert
            result.Should().Be("ImageSize                       : 1712x2288" + Environment.NewLine);

            await sut.DisposeAsync().ConfigureAwait(false);

            // just for fun
            _output.WriteLine(_image);
            _output.WriteLine(result);
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task RunExiftool_ShouldReturnEmpty_WhenQueriedTagDoesNotExist()
        {
            // arrange
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            sut.Initialize();

            // act
            Func<Task> act = async () => _ = await sut.ExecuteAsync(
                                                          new[]
                                                              {
                                                                  "-ExposureTime",
                                                                  _image,
                                                              });

            // assert
            act.Should().ThrowExactly<Exception>().WithMessage("Warning: IPTCDigest is not current. XMP may be out of sync - *");

            await sut.DisposeAsync().ConfigureAwait(false);
        }

        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyorWindows)]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [Performance]
        public async Task RunWithInputStreamTest()
        {
            // arrange
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            var sw = Stopwatch.StartNew();
            sut.Initialize();
            sw.Stop();
            _output.WriteLine($"It took {sw.Elapsed} to Initialize exiftool");

            // act
            sw.Reset();
            sw.Start();
            var version = string.Empty;
            for (var i = 0; i < REPEAT; i++)
            {
                version = await sut.GetVersionAsync().ConfigureAwait(false);
            }

            sw.Stop();
            await sut.DisposeAsync().ConfigureAwait(false);

            // assert
            _output.WriteLine($"It took {sw.Elapsed} to retrieve exiftool version {REPEAT} times");
            _output.WriteLine($"Version: {version}");
            version.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [Performance]
        public async Task DisposeAsyncShouldCancelAllPendingRequestsTest()
        {
            // arrange
            var tasks = new Task<string>[REPEAT];
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            var sw = Stopwatch.StartNew();
            sut.Initialize();
            sw.Stop();
            _output.WriteLine($"It took {sw.Elapsed} to Initialize exiftool");

            // act
            sw.Reset();
            sw.Start();
            for (var i = 0; i < REPEAT; i++)
            {
                tasks[i] = sut.GetVersionAsync();
            }

            sw.Stop();
            await sut.DisposeAsync().ConfigureAwait(false);

            // assert
            var countCancelled = 0;
            foreach (Task<string> t in tasks)
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    countCancelled++;
                }
            }

            countCancelled.Should().BeGreaterOrEqualTo(REPEAT / 2).And.NotBe(REPEAT);
            _output.WriteLine($"It took {sw.Elapsed} to retrieve exiftool version {REPEAT - countCancelled} times");
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task InitAndDisposeTest()
        {
            // arrange
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());

            // act
            sut.Initialize();
            await sut.DisposeAsync().ConfigureAwait(false);

            // assert
            // sut.IsClosed.Should().Be(true);
        }

        [Fact]
        public async Task RunExifToolWithThreeCommands()
        {
            // arrange
            var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
            sut.Initialize();

            // act
            Task<string> task1 = sut.ExecuteAsync(_image);
            Task<string> task2 = sut.ExecuteAsync(_image);
            Task<string> task3 = sut.ExecuteAsync(_image);

            // assert
            var result3 = await task3.ConfigureAwait(false);
            result3.Should().NotBeNullOrEmpty();

            var result2 = await task2.ConfigureAwait(false);
            result2.Should().NotBeNullOrEmpty();

            var result1 = await task1.ConfigureAwait(false);
            result1.Should().NotBeNullOrEmpty();

            await sut.DisposeAsync().ConfigureAwait(false);
        }
    }
}
