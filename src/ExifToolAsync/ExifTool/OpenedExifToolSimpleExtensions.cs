namespace ExifToolAsync.ExifTool
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class OpenedExifToolSimpleExtensions
    {
        public static Task<string> GetVersionAsync(this IExifTool @this, CancellationToken ct = default)
        {
            return @this.ExecuteAsync(new[] { "-ver" }, ct);
        }

        public static Task<string> ExecuteAsync(this IExifTool @this, string singleArg, CancellationToken ct = default)
        {
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}