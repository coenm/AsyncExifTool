namespace CoenM.ExifToolLibTest.Internals.Stream
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    using CoenM.ExifToolLib.Internals.Stream;
    using FluentAssertions;
    using TestHelper;
    using Xunit;

    public class ExifToolStdOutWriterTest : IDisposable
    {
        private readonly ExifToolStdOutWriter sut;
        private readonly List<DataCapturedArgs> capturedEvents;

        public ExifToolStdOutWriterTest()
        {
            capturedEvents = new List<DataCapturedArgs>();
            sut = new ExifToolStdOutWriter(Encoding.UTF8, OperatingSystemHelper.NewLine, 200);
            sut.Update += SutOnUpdate;
        }

        public void Dispose()
        {
            sut.Update -= SutOnUpdate;
        }

        [Fact]
        public void Ctor_ThrowsException_WhenEndLineIsEmpty()
        {
            // arrange
            var endLine = string.Empty;

            // act
            Action act = () => new ExifToolStdOutWriter(Encoding.UTF8, endLine, 200);

            // assert
            act.Should().Throw<ArgumentException>();
         }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void Ctor_ThrowsException_WhenEndLineIsNullOrEmpty()
        {
            // arrange
            string endLine = null;

            // act
            Action act = () => new ExifToolStdOutWriter(Encoding.UTF8, endLine, 200);

            // assert
            act.Should().Throw<ArgumentNullException>();
         }

        [Fact]
        public void ExifToolStdOutWriterCtorThrowsArgumentOutOfRangeWhenBufferSizeIsNegativeTest()
        {
            // arrange

            // act
            Action act = () => _ = new ExifToolStdOutWriter(Encoding.UTF32, OperatingSystemHelper.NewLine, -1);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
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

        [Theory]
        [ClassData(typeof(InvalidWriteInputWithoutException))]
        public void Write_ShouldNotThrow_WhenInputIsNotValid(byte[] buffer, int offset, int count)
        {
            // arrange

            // act
            sut.Write(buffer, offset, count);

            // assert
            capturedEvents.Should().BeEmpty();
        }


        [Fact]
        public void SingleWriteShouldNotFireEvent()
        {
            // arrange
            const string msg = "dummy data without delimiter";

            // act
            WriteMessageToSut(msg);

            // assert
            capturedEvents.Should().BeEmpty();
        }

        [Fact]
        public void ParseSingleMessage()
        {
            // arrange
            const string msg = "a b c\r\nd e f\r\n{ready0}\r\n";

            // act
            WriteMessageToSut(msg);

            // assert
            capturedEvents.Should().ContainSingle();
            capturedEvents.First().Key.Should().Be("0");
            capturedEvents.First().Data.Should().Be("a b c\r\nd e f".ConvertToOsString());
        }

        [Fact]
        public void ParseTwoMessagesInSingleWrite()
        {
            // arrange
            const string msg = "a b c\r\n{ready0}\r\nd e f\r\n{ready1}\r\nxyz";

            // act
            WriteMessageToSut(msg);

            // assert
            capturedEvents.Should().HaveCount(2);

            capturedEvents[0].Key.Should().Be("0");
            capturedEvents[0].Data.Should().Be("a b c");

            capturedEvents[1].Key.Should().Be("1");
            capturedEvents[1].Data.Should().Be("d e f");
        }

        [Fact]
        public void ParseTwoMessagesOverFourWrites()
        {
            // arrange
            const string msg1 = "a b c\r\nd e f\r\n{ready0}\r\nghi";
            const string msg2 = " jkl\r\n{re";
            const string msg3 = "ady";
            const string msg4 = "213";
            const string msg5 = "3}\r\n";

            // act
            WriteMessageToSut(msg1);
            WriteMessageToSut(msg2);
            WriteMessageToSut(msg3);
            WriteMessageToSut(msg4);
            WriteMessageToSut(msg5);

            // assert
            capturedEvents.Should().HaveCount(2)
                           .And.Contain(x => x.Key == "0" && x.Data == "a b c\r\nd e f".ConvertToOsString())
                           .And.Contain(x => x.Key == "2133" && x.Data == "ghi jkl".ConvertToOsString());
        }

        private void WriteMessageToSut(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message.ConvertToOsString());
            sut.Write(buffer, 0, buffer.Length);
        }

        private void SutOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            capturedEvents.Add(dataCapturedArgs);
        }

        private class InvalidWriteInputWithoutException : TheoryData<byte[], int, int>
        {
            public InvalidWriteInputWithoutException()
            {
                Add(null, 1, 1); // buffer is null
                Add(ValidBuffer, 0, 0); // count is zero
                Add(ValidBuffer, 0, -3); // count is negative
                Add(ValidBuffer, ValidBuffer.Length - 2, 0); // count is zero
                Add(ValidBuffer, ValidBuffer.Length + 1, 1); // offset is behind length of buffer
                /*Add(ValidBuffer, -1, ValidBuffer.Length); // offset is negative*/
            }

            private static byte[] ValidBuffer => Encoding.UTF8.GetBytes($"This is a message".ConvertToOsString());
        }

        private class InvalidWriteInputWithException : TheoryData<byte[], int, int>
        {
            public InvalidWriteInputWithException()
            {
                /* Add(ValidBuffer, ValidBuffer.Length - 1, 2); // offset + count is behind length of buffer */
                Add(ValidBuffer201, 0, ValidBuffer201.Length); // offset is behind length of buffer
                /* Add(ValidBuffer, -1, ValidBuffer.Length); // offset is negative */
            }

            private static byte[] ValidBuffer => Encoding.UTF8.GetBytes($"This is a message".ConvertToOsString());

            private static byte[] ValidBuffer201 => Encoding.UTF8.GetBytes(new string('a', 201).ConvertToOsString());
        }
    }
}
