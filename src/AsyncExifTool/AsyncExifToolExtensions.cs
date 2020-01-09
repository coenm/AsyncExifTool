namespace CoenM.ExifToolLib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals;
    using JetBrains.Annotations;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class AsyncExifToolExtensions
    {
        /// <summary>
        /// Get running ExifTool version.
        /// </summary>
        /// <param name="this">Instance of <see cref="AsyncExifTool"/> to use to execute this command. Cannot be <c>null</c>.</param>
        /// <param name="ct">Cancellation token. Optional. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>ExifTool result containing the version information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> is <c>null</c>.</exception>
        public static Task<string> GetVersionAsync([NotNull] this AsyncExifTool @this, CancellationToken ct = default)
        {
            return ExecuteAsync(@this, ExifToolArguments.Version, ct);
        }

        /// <summary>
        /// Execute single command on ExifTool instance.
        /// </summary>
        /// <param name="this">Instance of <see cref="AsyncExifTool"/> to use to execute this command. Cannot be <c>null</c>.</param>
        /// <param name="singleArg">The command to run on exiftool. For instance `-ver`. See the ExifTool website for all options.</param>
        /// <param name="ct">Cancellation token. Optional. Defaults to <see cref="CancellationToken.None"/>.</param>
        /// <returns>ExifTool result containing the version information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="this"/> is <c>null</c>.</exception>
        public static Task<string> ExecuteAsync([NotNull] this AsyncExifTool @this, string singleArg, CancellationToken ct = default)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}
