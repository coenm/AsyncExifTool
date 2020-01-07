namespace CoenM.ExifToolLib.Internals.MedallionShell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using JetBrains.Annotations;
    using Medallion.Shell;

    internal class MedallionShellAdapter : IShell, IDisposable
    {
        [NotNull]
        private readonly Command cmd;

        public MedallionShellAdapter(
            [NotNull] string executable,
            IEnumerable<string> args,
            [NotNull] Stream outputStream,
            [CanBeNull] Stream errorStream = null)
        {
            if (string.IsNullOrWhiteSpace(executable))
                throw new ArgumentNullException(nameof(executable));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            if (errorStream == null)
                cmd = Command.Run(executable, args)
                         .RedirectTo(outputStream);
            else
                cmd = Command.Run(executable, args)
                    .RedirectTo(outputStream)
                    .RedirectStandardErrorTo(errorStream);

            Task = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    var result = await cmd.Task.ConfigureAwait(false);
                    return new CommandResultAdapter(result) as IShellResult;
                }
                finally
                {
                    ProcessExited?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        [CanBeNull]
        public event EventHandler ProcessExited;

        public bool Finished => Task.IsCompleted;

        [NotNull]
        public Task<IShellResult> Task { get; }

        public async Task<bool> TryCancelAsync()
        {
            try
            {
                return await cmd.TrySignalAsync(CommandSignal.ControlC)
                         .ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Kill()
        {
            cmd.Kill();
        }

        public async Task WriteLineAsync([NotNull] string text)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (text != null)
                await cmd.StandardInput.WriteLineAsync(text).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Ignore(() => Task.Dispose());
            Ignore(() => ((IDisposable)cmd).Dispose());
        }

        private void Ignore(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
