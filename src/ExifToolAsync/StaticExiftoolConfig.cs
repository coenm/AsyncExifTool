namespace ExifToolAsync
{
    using Dawn;
    using JetBrains.Annotations;

    public class StaticExiftoolConfig : IExifToolConfig
    {
        public StaticExiftoolConfig([NotNull] string exifToolExe)
        {
            Guard.Argument(exifToolExe, nameof(exifToolExe)).NotNull().NotWhiteSpace();
            ExifToolExe = exifToolExe;
        }

        public string ExifToolExe { get; }
    }
}
