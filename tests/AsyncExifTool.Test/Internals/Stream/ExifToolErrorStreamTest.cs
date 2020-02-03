namespace CoenM.ExifToolLibTest.Internals.Stream
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using CoenM.ExifToolLib.Internals.Stream;
    using CoenM.ExifToolLib.Logging;
    using FluentAssertions;
    using TestHelper;
    using Xunit;

    public class ExifToolErrorStreamTest : IDisposable
    {
        private readonly ExifToolErrorStream sut;
        private readonly List<ErrorCapturedArgs> capturedEvents;

        public ExifToolErrorStreamTest()
        {
            capturedEvents = new List<ErrorCapturedArgs>();
            sut = new ExifToolErrorStream(new NullLogger(), Encoding.UTF8);
            sut.Error += SutOnError;
        }

        public void Dispose()
        {
            sut.Error -= SutOnError;
            sut?.Dispose();
        }

        [Fact]
        public void DefaultPropertiesShouldNoThrowAndDoNotDoAnythingTest()
        {
            sut.CanWrite.Should().BeTrue();
            sut.CanRead.Should().BeFalse();
            sut.CanSeek.Should().BeFalse();
            sut.Length.Should().Be(0);
            sut.Position.Should().Be(0);
        }

        [Fact]
        public void SetPositionShouldNotDoAnythingTest()
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
        public void FlushShouldNotDoAnythingAndDefinitelyNotThrowTest()
        {
            sut.Flush();
        }

        [Fact]
        public void SeekAlwaysReturnsZeroTest()
        {
            // arrange

            // act
            var result = sut.Seek(0, SeekOrigin.Begin);

            // assert
            result.Should().Be(0);
        }

        [Fact]
        public void SetLengthDoesNotDoAnythingTest()
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
        public void ReadThrowsTest()
        {
            // arrange
            var buffer = new byte[100];

            // act
            Action act = () => _ = sut.Read(buffer, 0, 100);

            // assert
            act.Should().Throw<NotSupportedException>();
        }

        [Theory]
        [ClassData(typeof(InvalidWriteInputWithException))]
        public void Write_ShouldThrow_WhenInputIsNotValid(byte[] buffer, int offset, int count)
        {
            // arrange

            // act
            Action act = () => sut.Write(buffer, offset, count);

            // assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SingleWriteShouldFireEvent()
        {
            // arrange
            const string msg = "dummy data 2";

            // act
            WriteMessageToSut(msg);

            // assert
            capturedEvents.Should().BeEquivalentTo(new ErrorCapturedArgs(msg));
        }

        private void WriteMessageToSut(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            sut.Write(buffer, 0, buffer.Length);
        }

        private void SutOnError(object sender, ErrorCapturedArgs dataCapturedArgs)
        {
            capturedEvents.Add(dataCapturedArgs);
        }

        private class InvalidWriteInputWithoutException : TheoryData<byte[], int, int>
        {
            public InvalidWriteInputWithoutException()
            {
                Add(null, 1, 1); // buffer is null
                Add(ValidBuffer, 0, 0); // count is zero
                Add(ValidBuffer, ValidBuffer.Length - 2, 0); // count is zero
                Add(ValidBuffer, ValidBuffer.Length + 1, 1); // offset is behind length of buffer
            }

            private static byte[] ValidBuffer => Encoding.UTF8.GetBytes($"This is a message".ConvertToOsString());
        }

        private class InvalidWriteInputWithException : TheoryData<byte[], int, int>
        {
            public InvalidWriteInputWithException()
            {
                Add(ValidBuffer, 0, -1); // count is negative
            }

            private static byte[] ValidBuffer => Encoding.UTF8.GetBytes($"This is a message".ConvertToOsString());
        }
    }
}
