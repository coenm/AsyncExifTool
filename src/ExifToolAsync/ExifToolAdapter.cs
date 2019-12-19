namespace ExifToolAsync
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Dawn;
    using ExifToolAsync.ExifTool;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ExifToolAdapter : IExifTool
    {
        private readonly OpenedExifTool exiftoolImpl;

        public ExifToolAdapter([NotNull] IExifToolConfig config)
        {
            Guard.Argument(config, nameof(config)).NotNull();

            exiftoolImpl = new OpenedExifTool(config.ExifToolExe);
            exiftoolImpl.Init();
        }

        public async Task<JObject> GetMetadataAsync(string filename)
        {
            var result = await exiftoolImpl.ExecuteAsync(filename).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
                return null;

            try
            {
                var jsonResult = JsonConvert.DeserializeObject(result);
                var jsonArray = jsonResult as JArray;
                if (jsonArray?.Count != 1)
                    return null;

                return jsonArray[0] as JObject;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            exiftoolImpl
                .DisposeAsync(new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token)
                .GetAwaiter()
                .GetResult();
        }
    }
}
