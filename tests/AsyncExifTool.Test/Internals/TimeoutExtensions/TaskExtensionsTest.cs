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
        private readonly ManualResetEvent _mre1;
        private readonly ManualResetEvent _mre2;
        private readonly CancellationTokenSource _cts;
        private Exception _exception;

        public TaskExtensionsTest()
        {
            _mre1 = new ManualResetEvent(false);
            _mre2 = new ManualResetEvent(false);
            _cts = new CancellationTokenSource();
            _exception = null;
        }

        // Tests if the WithWaitCancellation method throws an OperationCanceledException when CancellationToken is cancelled
        // and the main task didn't finish.
        [Fact]
        public async Task TaskWithWaitCancellationThrowsExceptionOnTokenCancellationTest()
        {
            // arrange
            var result = -1;

            // act
            Task<int> resultTask = DummyMethodReturning42WithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(_cts.Token);

            _mre1.WaitOne();
            _cts.Cancel();
            _mre2.Set();

            try
            {
                result = await resultTask;
            }
            catch (Exception e)
            {
                _exception = e;
            }

            // assert
            _exception.Should().NotBeNull();
            _exception.Should().BeOfType<OperationCanceledException>();
            _cts.IsCancellationRequested.Should().BeTrue();
            result.Should().Be(-1, "Result should be unchanged");
        }

        // Tests if the WithWaitCancellation method throws an OperationCanceledException when CancellationToken
        // is cancelled and the main task didn't finish.
        [Fact]
        public async Task TaskWithWaitCancellationThrowsExceptionOnTokenCancellationTest1()
        {
            // arrange

            // act
            Task resultTask = DummyMethodDoingNothingWithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(_cts.Token);

            _mre1.WaitOne();
            _cts.Cancel();
            _mre2.Set();

            try
            {
                await resultTask;
            }
            catch (Exception e)
            {
                _exception = e;
            }

            // assert
            _exception.Should().NotBeNull();
            _exception.Should().BeOfType<OperationCanceledException>();
            _cts.IsCancellationRequested.Should().BeTrue();
        }

        // Tests if the WithWaitCancellation method doesn't throws an Exception
        // when the 'main' task was already finished before the CancellationToken is cancelled.
        [Fact]
        public async Task TaskWithWaitCancellationDoesNotThrowsExceptionWhenMainTaskFinishesBeforeTokenCancellationTest()
        {
            // arrange
            var result = -1;

            // act
            Task<int> resultTask = DummyMethodReturning42WithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(_cts.Token);
            _mre2.Set();

            try
            {
                result = await resultTask;
                _cts.Cancel();
            }
            catch (Exception e)
            {
                _exception = e;
            }

            // assert
            _exception.Should().BeNull();
            _cts.IsCancellationRequested.Should().BeTrue();
            result.Should().Be(42);
        }

        // Tests if the WithWaitCancellation method doesn't throws an Exception
        // when the 'main' task was already finished before the CancellationToken is cancelled.
        [Fact]
        public async Task TaskWithWaitCancellationDoesNotThrowsExceptionWhenMainTaskFinishesBeforeTokenCancellationTest1()
        {
            // arrange

            // act
            Task resultTask = DummyMethodDoingNothingWithoutCancellationTokenSupportAsync()
                .WithWaitCancellation(_cts.Token);
            _mre2.Set();

            try
            {
                await resultTask;
                _cts.Cancel();
            }
            catch (Exception e)
            {
                _exception = e;
            }

            // assert
            _cts.IsCancellationRequested.Should().BeTrue();
            _exception.Should().BeNull();
        }

        private async Task<int> DummyMethodReturning42WithoutCancellationTokenSupportAsync()
        {
            await DummyMethodDoingNothingWithoutCancellationTokenSupportAsync();
            return 42;
        }

        private async Task DummyMethodDoingNothingWithoutCancellationTokenSupportAsync()
        {
            await Task.Yield();
            _mre1.Set();
            _mre2.WaitOne();
        }
    }
}
