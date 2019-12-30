namespace CoenM.ExifToolLib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class AsyncExifToolExtensions
    {
        public static Task<string> GetVersionAsync(this AsyncExifTool @this, CancellationToken ct = default)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.ExecuteAsync(new[] { "-ver" }, ct);
        }

        public static Task<string> ExecuteAsync(this AsyncExifTool @this, string singleArg, CancellationToken ct = default)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}
