namespace ExifToolAsync.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using JetBrains.Annotations;
    using Medallion.Shell;

    public class MedallionShellAdapter : IMedallionShell
    {
        [NotNull]
        private readonly Command cmd;

        public MedallionShellAdapter(
            string executable,
            IEnumerable<string> defaultArgs,
            [NotNull] Stream outputStream,
            [CanBeNull] Stream errorStream = null)
        {
            if (string.IsNullOrWhiteSpace(executable))
                throw new ArgumentNullException(nameof(executable));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            cmd = Command.Run(executable, defaultArgs)
                         .RedirectTo(outputStream);

            if (errorStream != null)
                cmd = cmd.RedirectStandardErrorTo(errorStream);

            Task = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    return await cmd.Task.ConfigureAwait(false);
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
        public Task<CommandResult> Task { get; }

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
    }
}
