namespace CoenM.ExifToolLibTest
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using CoenM.ExifToolLib;
    using FluentAssertions;
    using Xunit;

    public class AsyncExifToolExceptionTest
    {
        [Fact]
        public void RoundTripSerializeDeserialize()
        {
            // arrange
            var sut = new AsyncExifToolException(34, "std", "error text");
            var bf = new BinaryFormatter();
            using var ms = new MemoryStream();

            // act
            bf.Serialize(ms, sut);
            ms.Seek(0, 0);
            var result = (AsyncExifToolException)bf.Deserialize(ms);

            // assert
            sut.ToString().Should().BeEquivalentTo(result.ToString());
            sut.Should().BeEquivalentTo(result);
        }

        [Fact]
        public void AsyncExiftoolException_ShouldThrow_WhenConstructingWithInvalidSerializedData()
        {
            // arrange
            var fakeData = CoenM.Encoding.Z85Extended.Encode(new byte[20]);

            // act
            Action act = () => _ = Deserialize(fakeData);

            // assert
            act.Should().ThrowExactly<SerializationException>();
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void GetObjectData_ShouldThrow_WhenInfoArgumentIsNull()
        {
            // arrange
            SerializationInfo info = null;
            var sut = new AsyncExifToolException(35, "aaa", "bbb");

            // act
            Action act = () => sut.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        private static string Serialize(AsyncExifToolException ex)
        {
            var bf = new BinaryFormatter();
            using var ms = new MemoryStream();
            bf.Serialize(ms, ex);
            return Encoding.Z85Extended.Encode(ms.ToArray());
        }

        private static AsyncExifToolException Deserialize(string z85EncodedData)
        {
            var byes = Encoding.Z85Extended.Decode(z85EncodedData).ToArray();
            var bf = new BinaryFormatter();
            using var ms = new MemoryStream(byes);
            return (AsyncExifToolException)bf.Deserialize(ms);
        }
    }
}
