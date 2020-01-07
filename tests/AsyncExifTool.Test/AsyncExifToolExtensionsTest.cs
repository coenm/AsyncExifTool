namespace CoenM.ExifToolLibTest
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using FluentAssertions;
    using Xunit;

    using Sut = CoenM.ExifToolLib.AsyncExifToolExtensions;

    public class AsyncExifToolExtensionsTest
    {
        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void GetVersionAsync_ShouldThrow_WhenAsyncExifToolInstanceIsNull()
        {
            // arrange

            // act
            Action act = () => Sut.GetVersionAsync(null, CancellationToken.None);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void ExecuteAsync_ShouldThrow_WhenAsyncExifToolInstanceIsNull()
        {
            // arrange

            // act
            Action act = () => Sut.ExecuteAsync(null, string.Empty, CancellationToken.None);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
