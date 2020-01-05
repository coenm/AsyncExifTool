namespace CoenM.ExifToolLib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals;

    public static class AsyncExifToolExtensions
    {
        public static Task<string> GetVersionAsync(this AsyncExifTool @this, CancellationToken ct = default)
        {
            return ExecuteAsync(@this, ExifToolArguments.Version, ct);
        }

        public static Task<string> ExecuteAsync(this AsyncExifTool @this, string singleArg, CancellationToken ct = default)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}
