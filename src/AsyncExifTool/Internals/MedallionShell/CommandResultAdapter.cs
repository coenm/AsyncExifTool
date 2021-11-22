namespace CoenM.ExifToolLib.Internals.MedallionShell
{
    using CoenM.ExifToolLib.Internals.Guards;
    using JetBrains.Annotations;
    using Medallion.Shell;

    internal class CommandResultAdapter : IShellResult
    {
        [NotNull]
        private readonly CommandResult _commandResult;

        public CommandResultAdapter([NotNull] CommandResult commandResult)
        {
            Guard.NotNull(commandResult, nameof(commandResult));
            _commandResult = commandResult;
        }

        public int ExitCode => _commandResult.ExitCode;

        public bool Success => _commandResult.Success;

        public string StandardOutput => _commandResult.StandardOutput;

        public string StandardError => _commandResult.StandardError;
    }
}
