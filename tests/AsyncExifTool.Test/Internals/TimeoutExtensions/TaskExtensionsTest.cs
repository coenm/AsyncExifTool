namespace CoenM.ExifToolLibTest.Internals.TimeoutExtensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals.TimeoutExtensions;
    using FluentAssertions;
    using Xunit;

    public class TaskExtensionsTest
    {
        private readonly ManualResetEvent mre1;
        private readonly ManualResetEvent mre2;
        private readonly CancellationTokenSource cts;
        private Exception exception;

        public TaskExtensionsTest()
        {
            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);
            cts = new CancellationTokenSource();
            exception = null;
        }

        // Tests if the WithWaitCancellation method throws an OperationCanceledException when CancellationToken is cancelled
        // and the main task didn't finish.
        [Fact]
        public async Task TaskWithWaitCancellationThrowsExceptionOnTokenCancellationTest()
        {
            // arrange
            var result = -1;

            // act
            var resultTask = DummyMethodReturning42WithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(cts.Token);

            mre1.WaitOne();
            cts.Cancel();
            mre2.Set();

            try
            {
                result = await resultTask;
            }
            catch (Exception e)
            {
                exception = e;
            }

            // assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<OperationCanceledException>();
            cts.IsCancellationRequested.Should().BeTrue();
            result.Should().Be(-1, "Result should be unchanged");
        }

        // Tests if the WithWaitCancellation method throws an OperationCanceledException when CancellationToken
        // is cancelled and the main task didn't finish.
        [Fact]
        public async Task TaskWithWaitCancellationThrowsExceptionOnTokenCancellationTest1()
        {
            // arrange

            // act
            var resultTask = DummyMethodDoingNothingWithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(cts.Token);

            mre1.WaitOne();
            cts.Cancel();
            mre2.Set();

            try
            {
                await resultTask;
            }
            catch (Exception e)
            {
                exception = e;
            }

            // assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<OperationCanceledException>();
            cts.IsCancellationRequested.Should().BeTrue();
        }

        // Tests if the WithWaitCancellation method doesn't throws an Exception
        // when the 'main' task was already finished before the CancellationToken is cancelled.
        [Fact]
        public async Task TaskWithWaitCancellationDoesNotThrowsExceptionWhenMainTaskFinishesBeforeTokenCancellationTest()
        {
            // arrange
            var result = -1;

            // act
            var resultTask = DummyMethodReturning42WithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(cts.Token);
            mre2.Set();

            try
            {
                result = await resultTask;
                cts.Cancel();
            }
            catch (Exception e)
            {
                exception = e;
            }

            // assert
            exception.Should().BeNull();
            cts.IsCancellationRequested.Should().BeTrue();
            result.Should().Be(42);
        }

        // Tests if the WithWaitCancellation method doesn't throws an Exception
        // when the 'main' task was already finished before the CancellationToken is cancelled.
        [Fact]
        public async Task TaskWithWaitCancellationDoesNotThrowsExceptionWhenMainTaskFinishesBeforeTokenCancellationTest1()
        {
            // arrange

            // act
            var resultTask = DummyMethodDoingNothingWithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(cts.Token);
            mre2.Set();

            try
            {
                await resultTask;
                cts.Cancel();
            }
            catch (Exception e)
            {
                exception = e;
            }

            // assert
            cts.IsCancellationRequested.Should().BeTrue();
            exception.Should().BeNull();
        }

        private async Task<int> DummyMethodReturning42WithoutCancellationTokenSupportAsync()
        {
            await DummyMethodDoingNothingWithoutCancellationTokenSupportAsync();
            return 42;
        }

        private async Task DummyMethodDoingNothingWithoutCancellationTokenSupportAsync()
        {
            await Task.Yield();
            mre1.Set();
            mre2.WaitOne();
        }
    }
}
