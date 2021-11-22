namespace CoenM.ExifToolLib.Internals.TimeoutExtensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        // http://stackoverflow.com/questions/14524209/what-is-the-correct-way-to-cancel-an-async-operation-that-doesnt-accept-a-cance/14524565#14524565
        public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Register with the cancellation token.
            using (ct.Register(callback => (callback as TaskCompletionSource<bool>)?.TrySetResult(true), tcs))
            {
                // If the task waited on is the cancellation token...
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(ct);
                }
            }

            // Wait for one or the other to complete.
            return await task.ConfigureAwait(false);
        }

        public static async Task WithWaitCancellation(this Task task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Register with the cancellation token.
            using (ct.Register(callback => (callback as TaskCompletionSource<bool>)?.TrySetResult(true), tcs))
            {
                // If the task waited on is the cancellation token...
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(ct);
                }
            }

            // Wait for one or the other to complete.
            await task.ConfigureAwait(false);
        }
    }
}
