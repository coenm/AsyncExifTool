namespace CoenM.ExifToolLibTest
{
    using System;
    using System.Diagnostics;
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

    public class AsyncExifToolSimpleIntegrationTest
    {
        private const int Repeat = 100;
        private readonly string image;

        private readonly ITestOutputHelper output;

        public AsyncExifToolSimpleIntegrationTest(ITestOutputHelper output)
        {
            this.output = output;

            image = Directory
                     .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            image.Should().NotBeNullOrEmpty("Image should exist on system.");
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task RunExiftoolForVersionAndImageTest()
        {
            // arrange
            var sut = new AsyncExifTool(ExifToolSystemConfiguration.ExifToolExecutable);
            sut.Init();

            // act
            var version = await sut.GetVersionAsync().ConfigureAwait(false);
            var result = await sut.ExecuteAsync(image).ConfigureAwait(false);

            // assert
            version.Should().NotBeNullOrEmpty();
            result.Should().NotBeNullOrEmpty();

            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).ConfigureAwait(false);

            // just for fun
            output.WriteLine(version);
            output.WriteLine(result);
        }

        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyorWindows)]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [Performance]
        public async Task RunWithInputStreamTest()
        {
            // arrange
            var sut = new AsyncExifTool(ExifToolSystemConfiguration.ExifToolExecutable);
            var sw = Stopwatch.StartNew();
            sut.Init();
            sw.Stop();
            output.WriteLine($"It took {sw.Elapsed.ToString()} to Initialize exiftool");

            // act
            sw.Reset();
            sw.Start();
            var version = string.Empty;
            for (var i = 0; i < Repeat; i++)
                version = await sut.GetVersionAsync().ConfigureAwait(false);
            sw.Stop();
            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).ConfigureAwait(false);

            // assert
            output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {Repeat} times");
            output.WriteLine($"Version: {version}");
            version.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [Performance]
        public async Task DisposeAsyncShouldCancelAllPendingRequestsTest()
        {
            // arrange
            var tasks = new Task<string>[Repeat];
            var sut = new AsyncExifTool(ExifToolSystemConfiguration.ExifToolExecutable);
            var sw = Stopwatch.StartNew();
            sut.Init();
            sw.Stop();
            output.WriteLine($"It took {sw.Elapsed.ToString()} to Initialize exiftool");

            // act
            sw.Reset();
            sw.Start();
            for (var i = 0; i < Repeat; i++)
                tasks[i] = sut.GetVersionAsync();
            sw.Stop();
            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token).ConfigureAwait(false);

            // assert
            var count = 0;
            foreach (var t in tasks)
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    count++;
                }
            }

            count.Should().BeGreaterOrEqualTo(Repeat / 2).And.NotBe(Repeat);
            output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {Repeat} times");
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        public async Task InitAndDisposeTest()
        {
            // arrange
            var sut = new AsyncExifTool(ExifToolSystemConfiguration.ExifToolExecutable);

            // act
            sut.Init();
            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).ConfigureAwait(false);

            // assert
            // sut.IsClosed.Should().Be(true);
        }

        [Fact]
        public async Task RunExifToolWithThreeCommands()
        {
            // arrange
            var sut = new AsyncExifTool(ExifToolSystemConfiguration.ExifToolExecutable);
            sut.Init();

            // act
            var task1 = sut.ExecuteAsync(image);
            var task2 = sut.ExecuteAsync(image);
            var task3 = sut.ExecuteAsync(image);

            // assert
            var result3 = await task3.ConfigureAwait(false);
            result3.Should().NotBeNullOrEmpty();

            var result2 = await task2.ConfigureAwait(false);
            result2.Should().NotBeNullOrEmpty();

            var result1 = await task1.ConfigureAwait(false);
            result1.Should().NotBeNullOrEmpty();

            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).ConfigureAwait(false);
        }
    }
}
