namespace CoenM.ExifToolLibTest
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using CoenM.ExifToolLib;
    using FluentAssertions;
    using Xunit;

    public class AsyncExifToolInitialisationExceptionTest
    {
        [Fact]
        public void RoundTripSerializeDeserialize()
        {
            // arrange
            var innerException = new ApplicationException("inner test message");
            var sut = new AsyncExifToolInitialisationException("test message", innerException);
            var bf = new BinaryFormatter();
            using var ms = new MemoryStream();

            // act
            bf.Serialize(ms, sut);
            ms.Seek(0, 0);
            var result = (AsyncExifToolInitialisationException)bf.Deserialize(ms);

            // assert
            sut.ToString().Should().BeEquivalentTo(result.ToString());
            sut.Should().BeEquivalentTo(result);
            sut.InnerException.Should().BeEquivalentTo(innerException);
        }
    }
}
