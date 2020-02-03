namespace CoenM.ExifToolLib.Internals.MedallionShell
{
    using CoenM.ExifToolLib.Internals.Guards;

    using JetBrains.Annotations;
    using Medallion.Shell;

    internal class CommandResultAdapter : IShellResult
    {
        [NotNull] private readonly CommandResult commandResult;

        public CommandResultAdapter([NotNull] CommandResult commandResult)
        {
            Guard.NotNull(commandResult, nameof(commandResult));
            this.commandResult = commandResult;
        }

        public int ExitCode => commandResult.ExitCode;

        public bool Success => commandResult.Success;

        public string StandardOutput => commandResult.StandardOutput;

        public string StandardError => commandResult.StandardError;
    }
}
