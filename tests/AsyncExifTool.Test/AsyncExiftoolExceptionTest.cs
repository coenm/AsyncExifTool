namespace CoenM.ExifToolLibTest
{
    using System;
    using System.Collections.Generic;
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

        [Theory]
        [MemberData(nameof(ExceptionsWithSerializedRepresentation))]
        public void AsyncExiftoolException_IsSerializable(AsyncExifToolException sut, string expectedSerializedData)
        {
            // arrange

            // act
            var result = Serialize(sut);

            // assert
            result.Should().Be(expectedSerializedData);
        }

        [Theory]
        [MemberData(nameof(ExceptionsWithSerializedRepresentation))]
        public void AsyncExiftoolException_IsDeserializable(AsyncExifToolException expected, string serializedData)
        {
            // arrange

            // act
            var result = Deserialize(serializedData);

            // assert
            result.Should().BeEquivalentTo(expected);
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
            byte[] byes = Encoding.Z85Extended.Decode(z85EncodedData);
            var bf = new BinaryFormatter();
            using var ms = new MemoryStream(byes);
            return (AsyncExifToolException)bf.Deserialize(ms);
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Z85 encoded string")]
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "For now it is okay.")]
        public static IEnumerable<object[]> ExceptionsWithSerializedRepresentation =>
            new List<object[]>
            {
                new object[]
                {
                    new AsyncExifToolException(41, "aaa", "bbb"),
                    "009610rr90@@A910000001n]e002zMB9RUkmr&JUrbVX>efE#8A=U@kzAPWaf!LdjfF1tlB.(exA+dyKwP-9nvqEFqp<3k-x>6!EC#OY$wO%UGB.><$0rr91c{}]:zCq-LCXH+?z/oC=x<@XXB9RUkmr&JUBz(8lmr&rNAcb/jzv=^F00dlCwQ4{h26j(%z/{daB97@7A=IENy*?yapgn0J2tF=JB75Y)1vMWuvgC>DzE)&SCW>r7BzkVh2s@AxA8/!f5l0WVv@24:vpJ>EBAh8kxbjxRzeTJcq/(CTyGbs.v}wOXA+PA75k)7Rz//L?BywV{nP56ICN)k/v}xR3x(v(-wPRQ7wcUU6wPJ0gBoUVZB-H#64oD$WB8L}.BZ]h#BAmAZ0rAi40%nM80rrc20s5(o9-Eu9By/qwlVl^SwN/*@z/fRGnKNo5BzkVhvrcS8q*ACawO<4*CW>r7BzkVh2NyFk000010000F000060@@r317tRD1{s]a00uXtvG(#o001Amz!i/Xe>720w(JX!y!OLLe<TM7zEVaWx>zE9z!}<!v}xR3x(v>}1onA43jmaE3ig5a015Yv6mZz%0b",
                },
                new object[]
                {
                    new AsyncExifToolException(30, "xxx", null), /* standard error is null */
                    "009610rr90@@A910000001n]e002zMB9RUkmr&JUrbVX>efE#8A=U@kzAPWaf!LdjfF1tlB.(exA+dyKwP-9nvqEFqp<3k-x>6!EC#OY$wO%UGB.><$0rr91c{}]:zCq-LCXH+?z/oC=x<@XXB9RUkmr&JUBz(8lmr&rNAcb/jzv=^F00dlCwQ4{h26j(%z/{daB97@7A=IENy*?yapgn0J2tF=JB75Y)1vMWuvgC>DzE)&SCW>r7BzkVh2s@AxA8/!f5l0WVv@24:vpJ>EBAh8kxbjxRzeTJcq/(CTyGbs.v}wOXA+PA75k)7Rz//L?BywV{nP56ICN)k/v}xR3x(v(-wPRQ7wcUU6wPJ0gBoUVZB-H#64oD$WB8L}.BZ]h#BAmAZ0rJo50%nM80rrc20s5(o9-Eu9By/qwlVl^SwN/*@z/fRGnKNo5BzkVhvrcS8q*ACawO<4*CW>r7BzkVh2NyFk000010000u000060@@r319/6n3i*Rk001Amz!i/Xe>720w(JX!y!OLLe<TM7zEVaWx>zE9z!}<!v}xR3x(v>@3jmaE3ig5a015Yv6mZz%0b",
                },
                new object[]
                {
                    new AsyncExifToolException(30, null, null), /* standard output and standard error are null */
                    "009610rr90@@A910000001n]e002zMB9RUkmr&JUrbVX>efE#8A=U@kzAPWaf!LdjfF1tlB.(exA+dyKwP-9nvqEFqp<3k-x>6!EC#OY$wO%UGB.><$0rr91c{}]:zCq-LCXH+?z/oC=x<@XXB9RUkmr&JUBz(8lmr&rNAcb/jzv=^F00dlCwQ4{h26j(%z/{daB97@7A=IENy*?yapgn0J2tF=JB75Y)1vMWuvgC>DzE)&SCW>r7BzkVh2s@AxA8/!f5l0WVv@24:vpJ>EBAh8kxbjxRzeTJcq/(CTyGbs.v}wOXA+PA75k)7Rz//L?BywV{nP56ICN)k/v}xR3x(v(-wPRQ7wcUU6wPJ0gBoUVZB-H#64oD$WB8L}.BZ]h#BAmAZ0S&x60%nM80rrc20s5(o9-Eu9By/qwlVl^SwN/*@z/fRGnKNo5BzkVhvrcS8q*ACawO<4*CW>r7BzkVh2NyFk000010000u0000a3i*Oj001Amz!i/Xe>720w(JX!y!OLLe<TM7zEVaWx>zE9z!}<!v}xR3x(v>@3jmaE3ig5a015Yv6mZz%0b",
                },
            };
    }
}
