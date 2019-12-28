namespace ExifToolAsync
{
    using System.Collections.Generic;
    using System.Text;

    public interface IOpenedExifToolConfiguration
    {
        string ExifToolFullFilename { get; }
        
        Encoding ExifToolEncoding { get; }

        List<string> Arguments { get; }

        string ExifToolEndLine { get; }
    }
}
