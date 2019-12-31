﻿namespace CoenM.ExifToolLib.Internals.AsyncManualResetEvent
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using Nito.AsyncEx;

    internal static class AsyncManualResetEventExtensions
    {
        /// <summary>
        /// Asynchronously waits for this event to be set or for the wait to be canceled.
        /// </summary>
        /// <param name="this">The AsyncManualResetEvent</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
        /// <returns><c>true</c> if the current instance receives a signal before the <paramref name="cancellationToken"/> is set, <c>false</c> otherwise.</returns>
        public static async Task<bool> WaitOneAsync([NotNull] this AsyncManualResetEvent @this, CancellationToken cancellationToken)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            return await WaitOneAsyncImpl(@this, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously waits for this event to be set or for the wait to be canceled.
        /// </summary>
        /// <param name="this">The AsyncManualResetEvent</param>
        /// <param name="timeout">A TimeSpan that represents the time to wait before returning.</param>
        /// <returns><c>true</c> if the current instance receives a signal within the <paramref name="timeout"/> Timespan, <c>false</c> otherwise.</returns>
        public static async Task<bool> WaitOneAsync([NotNull] this AsyncManualResetEvent @this, TimeSpan timeout)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            using var cts = new CancellationTokenSource(timeout);
            return await WaitOneAsyncImpl(@this, cts.Token).ConfigureAwait(false);
        }

        private static async Task<bool> WaitOneAsyncImpl([NotNull] AsyncManualResetEvent asyncManualResetEvent, CancellationToken cancellationToken)
        {
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
