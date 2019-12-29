namespace CoenM.ExifToolLib
{
    using System.Collections.Generic;
    using System.Text;

    public sealed class AsyncExifToolConfiguration
    {
        public AsyncExifToolConfiguration(string exifToolFullFilename, Encoding exifToolEncoding, List<string> arguments, string exifToolEndLine)
        {
            ExifToolFullFilename = exifToolFullFilename;
            ExifToolEncoding = exifToolEncoding;
            Arguments = arguments;
            ExifToolEndLine = exifToolEndLine;
        }

        public string ExifToolFullFilename { get; }

        public string ExifToolEndLine { get; }

        public Encoding ExifToolEncoding { get; }

        public List<string> Arguments { get; }
    }
}
