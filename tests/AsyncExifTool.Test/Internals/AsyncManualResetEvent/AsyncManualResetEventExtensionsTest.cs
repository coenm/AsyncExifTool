namespace CoenM.ExifToolLibTest.Internals.AsyncManualResetEvent
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;
    using Nito.AsyncEx;
    using Xunit;

    using Sut = CoenM.ExifToolLib.Internals.AsyncManualResetEvent.AsyncManualResetEventExtensions;

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Readability test")]
    public class AsyncManualResetEventExtensionsTest
    {
        [Fact]
        public async Task WaitOneAsync_WithCancellationToken_ShouldReturnTrue_WhenResetBeforeCancelled()
        {
            // arrange
            var amre = new AsyncManualResetEvent(false);

            // act
            var resultTask = Sut.WaitOneAsync(amre, CancellationToken.None);
            await Task.Delay(100);
            amre.Set();
            var result = await resultTask;

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task WaitOneAsync_WithCancellationToken_ShouldReturnFalse_WhenCancellationTokenIsSet()
        {
            // arrange
            var amre = new AsyncManualResetEvent(false);
            using var cts = new CancellationTokenSource();

            // act
            var resultTask = Sut.WaitOneAsync(amre, cts.Token);
            cts.CancelAfter(100);
            var result = await resultTask;

            // assert
            result.Should().BeFalse();
            amre.IsSet.Should().BeFalse("Task was cancelled before amre was set.");
        }

        [Fact]
        public void WaitOneAsync_WithCancellationToken_ShouldThrow_WhenInputIsNull()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await Sut.WaitOneAsync(null, CancellationToken.None);

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public async Task WaitOneAsync_WithTimeSpan_ShouldReturnTrue_WhenResetBeforeTimedOut()
        {
            // arrange
            var amre = new AsyncManualResetEvent(false);
            var timeoutTimespan = TimeSpan.FromDays(1);

            // act
            var resultTask = Sut.WaitOneAsync(amre, timeoutTimespan);
            await Task.Delay(100);
            amre.Set();
            var result = await resultTask;

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task WaitOneAsync_WithTimeSpan_ShouldReturnFalse_WhenCancellationTokenIsSet()
        {
            // arrange
            var amre = new AsyncManualResetEvent(false);
            var timeoutTimespan = TimeSpan.FromMilliseconds(100);

            // act
            var result = await Sut.WaitOneAsync(amre, timeoutTimespan);

            // assert
            result.Should().BeFalse();
            amre.IsSet.Should().BeFalse("timeout kicked in before amre was set.");
        }

        [Fact]
        public void WaitOneAsync_WithTimeSpan_ShouldThrow_WhenInputIsNull()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await Sut.WaitOneAsync(null, TimeSpan.MaxValue);

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }
    }
}
