namespace CoenM.ExifToolLibTest.Internals.Stream
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CoenM.ExifToolLib.Internals.Stream;
    using FluentAssertions;
    using TestHelper;
    using Xunit;

    public class ExifToolStdErrWriterTest : IDisposable
    {
        private readonly ExifToolStdErrWriter sut;
        private readonly List<ErrorCapturedArgs> capturedEvents;

        public ExifToolStdErrWriterTest()
        {
            capturedEvents = new List<ErrorCapturedArgs>();
            sut = new ExifToolStdErrWriter(Encoding.UTF8);
            sut.Error += SutOnError;
        }

        public void Dispose()
        {
            sut.Error -= SutOnError;
        }

        [Theory]
        [ClassData(typeof(InvalidWriteInputWithoutException))]
        public void Write_ShouldDoNothing_WhenInputIsNotValid(byte[] buffer, int offset, int count)
        {
            // arrange

            // act
            sut.Write(buffer, offset, count);

            // assert
            capturedEvents.Should().BeEmpty();
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
                Add(ValidBuffer, 0, -1); // count is negative
                Add(ValidBuffer, 0, 0); // count is zero
                Add(ValidBuffer, ValidBuffer.Length - 2, 0); // count is zero
                Add(ValidBuffer, ValidBuffer.Length + 1, 1); // offset is behind length of buffer
            }

            private static byte[] ValidBuffer => Encoding.UTF8.GetBytes($"This is a message".ConvertToOsString());
        }
    }
}
