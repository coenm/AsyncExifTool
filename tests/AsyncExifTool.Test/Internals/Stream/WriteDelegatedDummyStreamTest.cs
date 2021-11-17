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
        private readonly WriteDelegatedDummyStream _sut;
        private readonly IBytesWriter _bytesWriter;

        public WriteDelegatedDummyStreamTest()
        {
            _bytesWriter = A.Fake<IBytesWriter>();
            _sut = new WriteDelegatedDummyStream(_bytesWriter);
        }

        public void Dispose()
        {
            _sut.Dispose();
        }

        [Fact]
        public void Write_ShouldForwardToBytesWriter()
        {
            // arrange
            var buffer = new byte[100];

            // act
            _sut.Write(buffer, 0, 100);

            // assert
            A.CallTo(() => _bytesWriter.Write(buffer, 0, 100)).MustHaveHappenedOnceExactly();
            A.CallTo(_bytesWriter).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Write_ShouldNotCatchException_WhenBytesWriterThrows()
        {
            // arrange
            A.CallTo(() => _bytesWriter.Write(A<byte[]>._, A<int>._, A<int>._))
                .Throws(new Exception("thrown by test"));

            var buffer = new byte[100];

            // act
            Action act = () => _sut.Write(buffer, 0, 100);

            // assert
            act.Should().ThrowExactly<Exception>().WithMessage("thrown by test");
            A.CallTo(() => _bytesWriter.Write(buffer, 0, 100)).MustHaveHappenedOnceExactly();
            A.CallTo(_bytesWriter).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void DefaultPropertiesShouldNoThrowAndDoNotDoAnythingTest()
        {
            _sut.CanWrite.Should().BeTrue();
            _sut.CanRead.Should().BeFalse();
            _sut.CanSeek.Should().BeFalse();
            _sut.Length.Should().Be(0);
            _sut.Position.Should().Be(0);
            A.CallTo(_bytesWriter).MustNotHaveHappened();
        }

        [Fact]
        public void SetPosition_ShouldNotDoAnything()
        {
            // arrange

            // assume
            _sut.Position.Should().Be(0);

            // act
            _sut.Position = 100;

            // assert
            _sut.Position.Should().Be(0);
        }

        [Fact]
        public void Flush_ShouldNotDoAnythingAndDefinitelyNotThrow()
        {
            _sut.Flush();
        }

        [Fact]
        public void Seek_ShouldAlwaysReturnZero()
        {
            // arrange

            // act
            var result = _sut.Seek(0, SeekOrigin.Begin);

            // assert
            result.Should().Be(0);
        }

        [Fact]
        public void SetLength_ShouldNotDoAnything()
        {
            // arrange

            // assume
            _sut.Length.Should().Be(0);

            // act
            _sut.SetLength(100);

            // assert
            _sut.Length.Should().Be(0);
        }

        [Fact]
        public void Read_ShouldReturnZero()
        {
            // arrange
            var buffer = new byte[100];

            // act
            var result = _sut.Read(buffer, 0, 100);

            // assert
            result.Should().Be(0);
        }
    }
}
