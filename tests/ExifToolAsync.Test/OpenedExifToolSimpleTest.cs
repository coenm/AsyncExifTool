using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExifToolAsyncTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ExifToolAsync;
    using ExifToolAsync.Internals;
    using FakeItEasy;
    using FluentAssertions;
    using TestHelper;
    using Xunit;

    public class OpenedExifToolSimpleTest
    {
        private readonly OpenedExifTool sut;
        private readonly IFakeExifTool fakeFakeExifTool;
        private readonly IShell shell;
        private readonly List<string> calledArguments;

        public OpenedExifToolSimpleTest()
        {
            calledArguments = new List<string>();
            shell = A.Fake<IShell>();
            sut = new TestableOpenedFakeExifTool(shell);
            fakeFakeExifTool = sut as IFakeExifTool;

            A.CallTo(() => shell.WriteLineAsync(A<string>._)).Returns(Task.CompletedTask);
        }

        [Fact]
        public void ExecuteAsync_ShouldThrow_WhenNotInitialized()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null).ConfigureAwait(false);

            // assert
            act.Should().Throw<Exception>().WithMessage("Not initialized");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenDisposing()
        {
            // arrange
            var control = fakeFakeExifTool.Setup(ExifToolArguments.BoolFalse);
            sut.Init();
            var disposingTask = sut.DisposeAsync();

            // act
            control.Entered.WaitOne();
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null);

            // assert
            act.Should().Throw<Exception>().WithMessage("Disposing");

            control.Release.Set();
            shell.ProcessExited += Raise.WithEmpty();
            await disposingTask;
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrow_WhenDisposed()
        {
            // arrange

            // ExifToolArguments.StayOpen, ExifToolArguments.BoolFalse 
            A.CallTo(() => shell.WriteLineAsync(ExifToolArguments.BoolFalse))
                .ReturnsLazily(async call =>
                {
                    await Task.Yield();
                    shell.ProcessExited += Raise.WithEmpty();
                });

            sut.Init();
            await sut.DisposeAsync();

            // act
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null);

            // assert
            act.Should().Throw<Exception>().WithMessage("Disposed");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPassArgsToInnerShellAndFinishWihExecute()
        {
            // arrange
            var args = new[] { "abc", "def" };
            sut.Init();

            // act
            _ = await sut.ExecuteAsync(args, CancellationToken.None);

            // assert
            A.CallTo(() => shell.WriteLineAsync("abc")).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => shell.WriteLineAsync("def")).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => shell.WriteLineAsync("-execute1")).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnWithStreamResult()
        {
            // arrange
            var args = new[] { "abc", "def" };
            sut.Init();

            // act
            var result = await sut.ExecuteAsync(args, CancellationToken.None);

            // assert
            result.Should().Be("fake result abc\r\nfake result def");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldFinishAllRequestsAlthoughOneWasCancelled()
        {
            // arrange
            var controlExecute1 = fakeFakeExifTool.Setup("-execute1");
            var cts = new CancellationTokenSource();
            sut.Init();

            // act
            var resultTask1 = sut.ExecuteAsync(new[] { "a" }, CancellationToken.None);
            controlExecute1.Entered.WaitOne(); // make sure resultTask1 is executing and has 'entered' ExifTool
            var resultTask2 = sut.ExecuteAsync(new[] { "b" }, cts.Token);
            var resultTask3 = sut.ExecuteAsync(new[] { "c" }, CancellationToken.None);
            cts.Cancel(); // cancel second task
            controlExecute1.Release.Set(); // signal 'exiftool' to finish the request of task1.

            // assert
            (await resultTask1).Should().Be("fake result a");
            resultTask2.IsCanceled.Should().BeTrue();
            (await resultTask3).Should().Be("fake result c");
        }

        private interface IFakeExifTool
        {
            void WriteToStream(Span<byte> data);

            ExifToolCommandControl Setup(string key);
        }

        private class TestableOpenedFakeExifTool : OpenedExifTool, IFakeExifTool
        {
            private readonly IShell shell;
            private readonly Dictionary<string, ExifToolCommandControl> exiftoolControl;
            private Stream exifToolStream;

            public TestableOpenedFakeExifTool(IShell shell)
                : base("doesn't matter")
            {
                this.shell = shell;
                exiftoolControl = new Dictionary<string, ExifToolCommandControl>();
            }

            internal override IShell CreateShell(string exifToolPath, IEnumerable<string> defaultArgs, Stream outputStream, Stream errorStream)
            {
                exifToolStream = Stream.Synchronized(outputStream);

                A.CallTo(() => shell.WriteLineAsync(A<string>._))
                    .Invokes(async call =>
                    {
                        // await Task.Run(() =>
                        if (!(call.Arguments[0] is string text))
                            throw new ArgumentNullException("text");

                        if (exiftoolControl.TryGetValue(text, out var control))
                        {
                            control.Entered.Set();
                            await control.Release.AsTask();
                        }

                        if (text.StartsWith("-execute"))
                        {
                            var result = text.Replace("-execute", "{ready") + "}";
                            await exifToolStream.WriteAsync(Encoding.UTF8.GetBytes(result + OperatingSystemHelper.NewLine));
                        }
                        else
                        {
                            await exifToolStream.WriteAsync(Encoding.UTF8.GetBytes($"fake result {text}{OperatingSystemHelper.NewLine}"));
                        }
                    });

                return shell;
            }

            void IFakeExifTool.WriteToStream(Span<byte> data)
            {
                exifToolStream.Write(data);
            }

            ExifToolCommandControl IFakeExifTool.Setup(string key)
            {
                var control = new ExifToolCommandControl();
                exiftoolControl.TryAdd(key, control);
                return control;
            }
        }
    }

    internal class ExifToolCommandControl
    {
        public ExifToolCommandControl()
        {
            Entered = new ManualResetEvent(false);
            Release = new ManualResetEvent(false);
        }

        public ManualResetEvent Entered { get; }

        public ManualResetEvent Release { get; }
    }
}


public static class WaitHandleExtensions
{
    public static Task AsTask(this WaitHandle handle)
    {
        return AsTask(handle, Timeout.InfiniteTimeSpan);
    }

    public static Task AsTask(this WaitHandle handle, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<object>();
        var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
        {
            var localTcs = (TaskCompletionSource<object>)state;
            if (timedOut)
                localTcs.TrySetCanceled();
            else
                localTcs.TrySetResult(null);
        }, tcs, timeout, executeOnlyOnce: true);
        tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle)state).Unregister(null), registration, TaskScheduler.Default);
        return tcs.Task;
    }
}
