namespace ExifToolAsyncTest.Json
{
    using ExifToolAsync;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TestHelper.XUnit.Facts;
    using Xunit;

    public class NewtonsoftJsonExperiments
    {
        private const string JsonArray = "[{\r\n  \"SourceFile\": \"C:/Repositories/EagleEye/Images/data/1.jpg\",\r\n  \"ExifTool\": {\r\n    \"ExifToolVersion\": 10.25\r\n  },\r\n  \"File\": {\r\n    \"FileName\": \"1.jpg\",\r\n    \"Directory\": \"C:/Repositories/EagleEye/Images/data\",\r\n    \"FileSize\": \"801 kB\",\r\n    \"FileModifyDate\": \"2018:02:13 21:01:09+01:00\",\r\n    \"FileAccessDate\": \"2018:02:13 21:01:09+01:00\",\r\n    \"FileCreateDate\": \"2018:02:13 21:01:09+01:00\",\r\n    \"FilePermissions\": \"rw-rw-rw-\",\r\n    \"FileType\": \"JPEG\",\r\n    \"FileTypeExtension\": \"jpg\",\r\n    \"MIMEType\": \"image/jpeg\",\r\n    \"ExifByteOrder\": \"Big-endian (Motorola, MM)\",\r\n    \"CurrentIPTCDigest\": \"4760d97b8d4857c4f8cf4504b16a595d\",\r\n    \"ImageWidth\": 1712,\r\n    \"ImageHeight\": 2288,\r\n    \"EncodingProcess\": \"Baseline DCT, Huffman coding\",\r\n    \"BitsPerSample\": 8,\r\n    \"ColorComponents\": 3,\r\n    \"YCbCrSubSampling\": \"YCbCr4:4:0 (1 2)\"\r\n  },\r\n  \"EXIF\": {\r\n    \"ImageDescription\": \"happy puppy!\",\r\n    \"XResolution\": 72,\r\n    \"YResolution\": 72,\r\n    \"ResolutionUnit\": \"inches\",\r\n    \"Software\": \"Picasa\",\r\n    \"ModifyDate\": \"2018:02:07 22:27:46\",\r\n    \"YCbCrPositioning\": \"Centered\",\r\n    \"ExifVersion\": \"0231\",\r\n    \"DateTimeOriginal\": \"2034:02:01 12:21:12\",\r\n    \"CreateDate\": \"2034:02:01 12:21:12\",\r\n    \"ComponentsConfiguration\": \"Y, Cb, Cr, -\",\r\n    \"FlashpixVersion\": \"0100\",\r\n    \"ColorSpace\": \"Uncalibrated\",\r\n    \"ImageUniqueID\": \"32da8d4383d8922abdda96abd924a4d6\",\r\n    \"GPSVersionID\": \"2.2.0.0\",\r\n    \"GPSLatitudeRef\": \"North\",\r\n    \"GPSLatitude\": 40.736072,\r\n    \"GPSLongitudeRef\": \"West\",\r\n    \"GPSLongitude\": 73.994293,\r\n    \"GPSAltitudeRef\": \"Above Sea Level\",\r\n    \"GPSAltitude\": \"34 m\",\r\n    \"GPSTimeStamp\": \"17:21:12\",\r\n    \"GPSMapDatum\": \"WGS-84\",\r\n    \"GPSDateStamp\": \"2034:02:01\"\r\n  },\r\n  \"XMP\": {\r\n    \"XMPToolkit\": \"Image::ExifTool 10.78\",\r\n    \"CountryCode\": \"USA\",\r\n    \"Location\": \"Union Square\",\r\n    \"Description\": \"happy puppy!\",\r\n    \"Subject\": [\"dog\",\"new york\",\"puppy\"],\r\n    \"DateTimeOriginal\": \"2034:02:01 12:21:12-05:00\",\r\n    \"GPSAltitude\": \"34 m\",\r\n    \"GPSAltitudeRef\": \"Above Sea Level\",\r\n    \"GPSLatitude\": \"+40.736072\",\r\n    \"GPSLongitude\": -73.994293,\r\n    \"GPSMapDatum\": \"WGS-84\",\r\n    \"GPSDateTime\": \"2034:02:01 17:21:12Z\",\r\n    \"GPSVersionID\": \"2.2.0.0\",\r\n    \"City\": \"New York\",\r\n    \"Country\": \"United States\",\r\n    \"State\": \"New York\",\r\n    \"CreateDate\": \"2034:02:01 12:21:12-05:00\",\r\n    \"ModifyDate\": \"2018:02:07 22:27:46+01:00\"\r\n  },\r\n  \"IPTC\": {\r\n    \"EnvelopeRecordVersion\": 4,\r\n    \"CodedCharacterSet\": \"UTF8\",\r\n    \"ApplicationRecordVersion\": 4,\r\n    \"Keywords\": [\"dog\",\"new york\",\"puppy\"],\r\n    \"City\": \"New York\",\r\n    \"Sub-location\": \"Union Square\",\r\n    \"Province-State\": \"New York\",\r\n    \"Country-PrimaryLocationCode\": \"USA\",\r\n    \"Country-PrimaryLocationName\": \"United States\",\r\n    \"Caption-Abstract\": \"happy puppy!\"\r\n  },\r\n  \"Photoshop\": {\r\n    \"IPTCDigest\": \"d49c3c865c8e976cbcb1e5f50e3de45f\"\r\n  },\r\n  \"Composite\": {\r\n    \"GPSAltitude\": \"34 m Above Sea Level\",\r\n    \"GPSDateTime\": \"2034:02:01 17:21:12Z\",\r\n    \"GPSLatitude\": \"+40.736072\",\r\n    \"GPSLatitudeRef\": \"North\",\r\n    \"GPSLongitude\": -73.994293,\r\n    \"GPSLongitudeRef\": \"West\",\r\n    \"GPSPosition\": \"+40.736072, -73.994293\",\r\n    \"ImageSize\": \"1712x2288\",\r\n    \"Megapixels\": 3.9\r\n  }\r\n}]";
        private const string ExpectedExif = "{\r\n  \"ImageDescription\": \"happy puppy!\",\r\n  \"XResolution\": 72,\r\n  \"YResolution\": 72,\r\n  \"ResolutionUnit\": \"inches\",\r\n  \"Software\": \"Picasa\",\r\n  \"ModifyDate\": \"2018:02:07 22:27:46\",\r\n  \"YCbCrPositioning\": \"Centered\",\r\n  \"ExifVersion\": \"0231\",\r\n  \"DateTimeOriginal\": \"2034:02:01 12:21:12\",\r\n  \"CreateDate\": \"2034:02:01 12:21:12\",\r\n  \"ComponentsConfiguration\": \"Y, Cb, Cr, -\",\r\n  \"FlashpixVersion\": \"0100\",\r\n  \"ColorSpace\": \"Uncalibrated\",\r\n  \"ImageUniqueID\": \"32da8d4383d8922abdda96abd924a4d6\",\r\n  \"GPSVersionID\": \"2.2.0.0\",\r\n  \"GPSLatitudeRef\": \"North\",\r\n  \"GPSLatitude\": 40.736072,\r\n  \"GPSLongitudeRef\": \"West\",\r\n  \"GPSLongitude\": 73.994293,\r\n  \"GPSAltitudeRef\": \"Above Sea Level\",\r\n  \"GPSAltitude\": \"34 m\",\r\n  \"GPSTimeStamp\": \"17:21:12\",\r\n  \"GPSMapDatum\": \"WGS-84\",\r\n  \"GPSDateStamp\": \"2034:02:01\"\r\n}";

        [Fact]
        public void JsonJArrayTest()
        {
            // arrange

            // act
            var jsonArray = JArray.Parse(JsonArray.ConvertToOsString());
            var exif = jsonArray[0]["EXIF"];

            // assert
            exif.ToString().Should().Be(ExpectedExif.ConvertToOsString());
        }

        [Fact]
        public void DeserializeObjectAsObjectTest()
        {
            // arrange

            // act
            var jsonObject = JsonConvert.DeserializeObject(JsonArray.ConvertToOsString());
            var jsonArray = jsonObject as JArray;
            var firstItem = jsonArray?[0] as JObject;
            var exifItem = firstItem?["EXIF"] as JObject;
            var exif = exifItem?.ToString();

            // assert
            exif.Should().Be(ExpectedExif.ConvertToOsString());
        }

        [ConditionalHostFact(TestHost.Local, "Fragile on AppVeyor and Travis")]
        public void DeserializeObjectAsDynamicTest()
        {
            // arrange

            // act
            dynamic jsonDynamic = JsonConvert.DeserializeObject(JsonArray.ConvertToOsString());
            var exif = jsonDynamic[0].EXIF;

            // assert
            var exifString = (string)exif.ToString();
            exifString.Should().Be(ExpectedExif.ConvertToOsString());
        }
    }
}
