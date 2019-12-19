namespace ExifToolAsyncTest
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using ExifToolAsync;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using TestHelper;
    using Xunit;

    public class ExifToolAdapterTest : IDisposable
    {
        private readonly string imageFilename;
        private readonly ExifToolAdapter sut;

        public ExifToolAdapterTest()
        {
            imageFilename = Directory
                     .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            imageFilename.Should().NotBeNullOrEmpty();

            var config = new StaticExiftoolConfig(ExifToolSystemConfiguration.ExifToolExecutable);
            sut = new ExifToolAdapter(config);
        }

        [Fact]
        public void SutCanBeDisposedMultipleTimesTest()
        {
            // should not throw.
            sut.Dispose();
            sut.Dispose();
        }

        [Fact]
        public async Task GetMetadataAsyncWithPreparedImageShouldResultInExpectedJsonObjectTest()
        {
            // arrange
            const string expectedExif = "{\r\n  \"ImageDescription\": \"happy puppy!\",\r\n  \"XResolution\": 72,\r\n  \"YResolution\": 72,\r\n  \"ResolutionUnit\": \"inches\",\r\n  \"Software\": \"Picasa\",\r\n  \"ModifyDate\": \"2018:02:07 22:27:46\",\r\n  \"YCbCrPositioning\": \"Centered\",\r\n  \"ExifVersion\": \"0231\",\r\n  \"DateTimeOriginal\": \"2034:02:01 12:21:12\",\r\n  \"CreateDate\": \"2034:02:01 12:21:12\",\r\n  \"ComponentsConfiguration\": \"Y, Cb, Cr, -\",\r\n  \"FlashpixVersion\": \"0100\",\r\n  \"ColorSpace\": \"Uncalibrated\",\r\n  \"ImageUniqueID\": \"32da8d4383d8922abdda96abd924a4d6\",\r\n  \"GPSVersionID\": \"2.2.0.0\",\r\n  \"GPSLatitudeRef\": \"North\",\r\n  \"GPSLatitude\": 40.736072,\r\n  \"GPSLongitudeRef\": \"West\",\r\n  \"GPSLongitude\": 73.994293,\r\n  \"GPSAltitudeRef\": \"Above Sea Level\",\r\n  \"GPSAltitude\": \"34 m\",\r\n  \"GPSTimeStamp\": \"17:21:12\",\r\n  \"GPSMapDatum\": \"WGS-84\",\r\n  \"GPSDateStamp\": \"2034:02:01\"\r\n}";

            // act
            var result = await sut.GetMetadataAsync(imageFilename).ConfigureAwait(false);

            // assert
            result.Should().NotBeNull("Expected result should not be null");
            var exif = result["EXIF"] as JObject;
            exif.Should().NotBeNull();
            exif.ToString().Should().Be(expectedExif.ConvertToOsString());
        }

        public void Dispose()
        {
            sut?.Dispose();
        }
    }
}
