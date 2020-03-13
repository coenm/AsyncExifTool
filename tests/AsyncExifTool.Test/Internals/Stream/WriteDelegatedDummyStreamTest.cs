namespace CoenM.ExifToolLibTest.Internals.Stream
{
    using System;
    using System.IO;

    using CoenM.ExifToolLib.Internals.Stream;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class WriteDelegatedDummyStreamTest : IDisposable
    {
        private readonly WriteDelegatedDummyStream sut;
        private readonly IBytesWriter bytesWriter;

        public WriteDelegatedDummyStreamTest()
        {
            bytesWriter = A.Fake<IBytesWriter>();
            sut = new WriteDelegatedDummyStream(bytesWriter);
        }

        public void Dispose()
        {
            sut.Dispose();
        }

        [Fact]
        public void Write_ShouldForwardToBytesWriter()
        {
            // arrange
            var buffer = new byte[100];

            // act
            sut.Write(buffer, 0, 100);

            // assert
            A.CallTo(() => bytesWriter.Write(buffer, 0, 100)).MustHaveHappenedOnceExactly();
            A.CallTo(bytesWriter).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Write_ShouldNotCatchException_WhenBytesWriterThrows()
        {
            // arrange
            A.CallTo(() => bytesWriter.Write(A<byte[]>._, A<int>._, A<int>._))
                .Throws(new Exception("thrown by test"));

            var buffer = new byte[100];

            // act
            Action act = () => sut.Write(buffer, 0, 100);

            // assert
            act.Should().ThrowExactly<Exception>().WithMessage("thrown by test");
            A.CallTo(() => bytesWriter.Write(buffer, 0, 100)).MustHaveHappenedOnceExactly();
            A.CallTo(bytesWriter).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DefaultPropertiesShouldNoThrowAndDoNotDoAnythingTest()
        {
            sut.CanWrite.Should().BeTrue();
            sut.CanRead.Should().BeFalse();
            sut.CanSeek.Should().BeFalse();
            sut.Length.Should().Be(0);
            sut.Position.Should().Be(0);
            A.CallTo(bytesWriter).MustNotHaveHappened();
        }

        [Fact]
        public void SetPosition_ShouldNotDoAnything()
        {
            // arrange

            // assume
            sut.Position.Should().Be(0);

            // act
            sut.Position = 100;

            // assert
            sut.Position.Should().Be(0);
        }

        [Fact]
        public void Flush_ShouldNotDoAnythingAndDefinitelyNotThrow()
        {
            sut.Flush();
        }

        [Fact]
        public void Seek_ShouldAlwaysReturnZero()
        {
            // arrange

            // act
            var result = sut.Seek(0, SeekOrigin.Begin);

            // assert
            result.Should().Be(0);
        }

        [Fact]
        public void SetLength_ShouldNotDoAnything()
        {
            // arrange

            // assume
            sut.Length.Should().Be(0);

            // act
            sut.SetLength(100);

            // assert
            sut.Length.Should().Be(0);
        }

        [Fact]
        public void Read_ShouldReturnZero()
        {
            // arrange
            var buffer = new byte[100];

            // act
            var result = sut.Read(buffer, 0, 100);

            // assert
            result.Should().Be(0);
        }
    }
}
