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
        private readonly ExifToolStdErrWriter _sut;
        private readonly List<ErrorCapturedArgs> _capturedEvents;

        public ExifToolStdErrWriterTest()
        {
            _capturedEvents = new List<ErrorCapturedArgs>();
            _sut = new ExifToolStdErrWriter(Encoding.UTF8);
            _sut.Error += SutOnError;
        }

        public void Dispose()
        {
            _sut.Error -= SutOnError;
        }

        [Theory]
        [ClassData(typeof(InvalidWriteInputWithoutException))]
        public void Write_ShouldDoNothing_WhenInputIsNotValid(byte[] buffer, int offset, int count)
        {
            // arrange

            // act
            _sut.Write(buffer, offset, count);

            // assert
            _capturedEvents.Should().BeEmpty();
        }

        [Fact]
        public void SingleWriteShouldFireEvent()
        {
            // arrange
            const string MSG = "dummy data 2";

            // act
            WriteMessageToSut(MSG);

            // assert
            _capturedEvents.Should().BeEquivalentTo(new ErrorCapturedArgs(MSG));
        }

        private void WriteMessageToSut(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            _sut.Write(buffer, 0, buffer.Length);
        }

        private void SutOnError(object sender, ErrorCapturedArgs dataCapturedArgs)
        {
            _capturedEvents.Add(dataCapturedArgs);
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
