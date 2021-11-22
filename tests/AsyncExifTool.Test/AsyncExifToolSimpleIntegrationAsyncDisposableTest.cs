namespace CoenM.ExifToolLibTest
{
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

    public class AsyncExifToolSimpleIntegrationAsyncDisposableTest
    {
        private const int REPEAT = 100;
        private readonly string _image;
        private readonly ITestOutputHelper _output;

        public AsyncExifToolSimpleIntegrationAsyncDisposableTest(ITestOutputHelper output)
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
#if FEATURE_ASYNC_DISPOSABLE
            await using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#else
            using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#endif
            sut.Initialize();

            // act
            var version = await sut.GetVersionAsync().ConfigureAwait(false);
            var result = await sut.ExecuteAsync(_image).ConfigureAwait(false);

            // assert
            version.Should().NotBeNullOrEmpty();
            result.Should().NotBeNullOrEmpty();

            // just for fun
            _output.WriteLine(version);
            _output.WriteLine(result);
        }

        [ConditionalHostFact(TestHostMode.Skip, TestHost.AppVeyorWindows)]
        [Xunit.Categories.IntegrationTest]
        [ExifTool]
        [Performance]
        public async Task RunWithInputStreamTest()
        {
            // arrange
#if FEATURE_ASYNC_DISPOSABLE
            await using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#else
            using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#endif
            var sw = Stopwatch.StartNew();
            sut.Initialize();
            sw.Stop();
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to Initialize exiftool");

            // act
            sw.Reset();
            sw.Start();
            var version = string.Empty;
            for (var i = 0; i < REPEAT; i++)
            {
                version = await sut.GetVersionAsync().ConfigureAwait(false);
            }

            sw.Stop();

            // assert
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT} times");
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
            Stopwatch sw;
#if FEATURE_ASYNC_DISPOSABLE
            await using (var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create()))
#else
            using (var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create()))
#endif
            {
                sw = Stopwatch.StartNew();
                sut.Initialize();
                sw.Stop();
                _output.WriteLine($"It took {sw.Elapsed.ToString()} to Initialize exiftool");

                // act
                sw.Reset();
                sw.Start();
                for (var i = 0; i < REPEAT; i++)
                {
                    tasks[i] = sut.GetVersionAsync();
                }

                sw.Stop();
            }

            // assert
            var countCancelled = 0;
            foreach (Task<string> t in tasks)
            {
                try
                {
                    _output.WriteLine(await t.ConfigureAwait(false));
                }
                catch (TaskCanceledException)
                {
                    countCancelled++;
                }
            }

            countCancelled.Should().BeGreaterOrEqualTo(REPEAT / 2).And.NotBe(REPEAT);
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT - countCancelled} times");
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
#if FEATURE_ASYNC_DISPOSABLE
            await sut.DisposeAsync().ConfigureAwait(false);
#else
            await Task.Yield();
            sut.Dispose();
#endif

            // assert
            // sut.IsClosed.Should().Be(true);
        }

        [Fact]
        public async Task RunExifToolWithThreeCommands()
        {
            // arrange
#if FEATURE_ASYNC_DISPOSABLE
            await using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#else
            using var sut = new AsyncExifTool(AsyncExifToolConfigurationFactory.Create());
#endif
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
        }
    }
}
