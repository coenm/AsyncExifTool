namespace CoenM.ExifToolLib
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals;

    public static class AsyncExifToolExtensions
    {
        public static Task<string> GetVersionAsync([NotNull] this AsyncExifTool @this, CancellationToken ct = default)
        {
            return ExecuteAsync(@this, ExifToolArguments.Version, ct);
        }

        public static Task<string> ExecuteAsync([NotNull] this AsyncExifTool @this, string singleArg, CancellationToken ct = default)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}
