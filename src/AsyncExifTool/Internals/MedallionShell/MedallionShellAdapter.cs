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
        private readonly string _executable;
        private readonly List<string>? _args;
        private readonly Stream _outputStream;
        private readonly Stream _errorStream;
        private Command? _cmd;
        private bool _initialized;

        public MedallionShellAdapter(
            string executable,
            IEnumerable<string>? args,
            Stream outputStream,
            Stream errorStream)
        {
            Guard.NotNullOrWhiteSpace(executable, nameof(executable));
            Guard.NotNull(outputStream, nameof(outputStream));

            _executable = executable;
            _args = args?.ToList();
            _outputStream = outputStream;
            _errorStream = errorStream;
            Task = System.Threading.Tasks.Task.FromResult(new DummyShellResult() as IShellResult);
        }

        public event EventHandler? ProcessExited;

        public bool Finished => Task.IsCompleted;

        public Task<IShellResult> Task { get; private set; }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _cmd = Command.Run(_executable, _args)
                          .RedirectTo(_outputStream)
                          .RedirectStandardErrorTo(_errorStream);

            Task = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    CommandResult result = await _cmd.Task.ConfigureAwait(false);
                    return new CommandResultAdapter(result) as IShellResult;
                }
                finally
                {
                    ProcessExited?.Invoke(this, EventArgs.Empty);
                }
            });

            _initialized = true;
        }

        public async Task<bool> TryCancelAsync()
        {
            try
            {
                if (_cmd == null)
                {
                    return true;
                }

                return await _cmd.TrySignalAsync(CommandSignal.ControlC)
                                 .ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Kill()
        {
            _cmd?.Kill();
        }

        public async Task WriteLineAsync(string text)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (text != null)
            {
                Command? cmd = _cmd;
                if (cmd != null)
                {
                    await cmd.StandardInput.WriteLineAsync(text).ConfigureAwait(false);
                }
            }
        }

        public void Dispose()
        {
            Ignore(() => Task.Dispose());
            Ignore(() => ((IDisposable?)_cmd)?.Dispose());
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

            public string StandardOutput { get; } = string.Empty;

            public string StandardError { get; } = string.Empty;
        }
    }
}
