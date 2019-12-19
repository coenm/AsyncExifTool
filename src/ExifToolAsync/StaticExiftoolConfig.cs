namespace ExifToolAsync
{
    using System;

    using JetBrains.Annotations;

    public class StaticExiftoolConfig : IExifToolConfig
    {
        public StaticExiftoolConfig([NotNull] string exifToolExe)
        {
            if (string.IsNullOrWhiteSpace(exifToolExe))
                throw new ArgumentNullException(nameof(exifToolExe));
            
            ExifToolExe = exifToolExe;
        }

        public string ExifToolExe { get; }
    }
}
