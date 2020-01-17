namespace CoenM.ExifToolLib.Internals.MedallionShell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;
    using Medallion.Shell;

    internal class MedallionShellAdapter : IShell, IDisposable
    {
        [NotNull] private readonly string executable;
        [CanBeNull] private readonly List<string> args;
        [NotNull] private readonly Stream outputStream;
        [NotNull] private readonly Stream errorStream;
        [CanBeNull] private Command cmd;
        private bool initialized;

        public MedallionShellAdapter(
            [NotNull] string executable,
            [CanBeNull] IEnumerable<string> args,
            [NotNull] Stream outputStream,
            [NotNull] Stream errorStream)
        {
            Guard.NotNullOrWhiteSpace(executable, nameof(executable));
            Guard.NotNull(outputStream, nameof(outputStream));

            this.executable = executable;
            this.args = args?.ToList();
            this.outputStream = outputStream;
            this.errorStream = errorStream;
            Task = System.Threading.Tasks.Task.FromResult(new DummyShellResult() as IShellResult);
        }

        [CanBeNull]
        public event EventHandler ProcessExited;

        public bool Finished => Task.IsCompleted;

        [NotNull]
        public Task<IShellResult> Task { get; private set; }

        public void Initialize()
        {
            if (initialized)
                return;

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

            initialized = true;
        }

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
            if (text != null)
                await cmd.StandardInput.WriteLineAsync(text).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Ignore(() => Task.Dispose());
            Ignore(() => ((IDisposable)cmd).Dispose());
        }

        private static void Ignore(Action action)
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

        private class DummyShellResult : IShellResult
        {
            public int ExitCode { get; }

            public bool Success { get; }

            public string StandardOutput { get; }

            public string StandardError { get; }
        }
    }
}
