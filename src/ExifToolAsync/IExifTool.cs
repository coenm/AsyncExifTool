namespace ExifToolAsync
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal interface IExifTool : IDisposable
    {
        Task<JObject> GetMetadataAsync([NotNull] string filename);
    }
}
