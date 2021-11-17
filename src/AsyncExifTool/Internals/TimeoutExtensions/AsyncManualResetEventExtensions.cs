namespace CoenM.ExifToolLib.Internals.TimeoutExtensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;
    using Nito.AsyncEx;

    internal static class AsyncManualResetEventExtensions
    {
        // stupid workaround to extract xml documentation on release. research if https://www.gapotchenko.com/eazfuscator.net is an option.
#if !RELEASE
        /// <summary>
        /// Asynchronously waits for this event to be set or for the wait to be canceled.
        /// </summary>
        /// <param name="this">The AsyncManualResetEvent.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
        /// <returns><c>true</c> if the current instance receives a signal before the <paramref name="cancellationToken"/> is set, <c>false</c> otherwise.</returns>
#endif
        public static async Task<bool> WaitOneAsync([NotNull] this AsyncManualResetEvent @this, CancellationToken cancellationToken)
        {
            Guard.NotNull(@this, nameof(@this));
            return await WaitOneAsyncImpl(@this, cancellationToken).ConfigureAwait(false);
        }

#if !RELEASE
        /// <summary>
        /// Asynchronously waits for this event to be set or for the wait to be canceled.
        /// </summary>
        /// <param name="this">The AsyncManualResetEvent.</param>
        /// <param name="timeout">A TimeSpan that represents the time to wait before returning.</param>
        /// <returns><c>true</c> if the current instance receives a signal within the <paramref name="timeout"/> Timespan, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is invalid for creating a <see cref="CancellationTokenSource"/>.</exception>
#endif
        public static async Task<bool> WaitOneAsync([NotNull] this AsyncManualResetEvent @this, TimeSpan timeout)
        {
            Guard.NotNull(@this, nameof(@this));
            using var cts = new CancellationTokenSource(timeout);
            return await WaitOneAsyncImpl(@this, cts.Token).ConfigureAwait(false);
        }

        private static async Task<bool> WaitOneAsyncImpl([NotNull] AsyncManualResetEvent asyncManualResetEvent, CancellationToken cancellationToken)
        {
            DebugGuard.NotNull(asyncManualResetEvent, nameof(asyncManualResetEvent));

            try
            {
                await asyncManualResetEvent.WaitAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }
    }
}
