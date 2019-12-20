namespace ExifToolAsync.ExifTool
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class OpenedExifToolSimpleExtensions
    {
        public static Task<string> GetVersionAsync(this OpenedExifTool @this, CancellationToken ct = default)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.ExecuteAsync(new[] { "-ver" }, ct);
        }

        public static Task<string> ExecuteAsync(this OpenedExifTool @this, string singleArg, CancellationToken ct = default)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}
