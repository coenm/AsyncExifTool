namespace CoenM.ExifToolLib.Internals.MedallionShell
{
    using System;
    using JetBrains.Annotations;
    using Medallion.Shell;

    internal class CommandResultAdapter : IShellResult
    {
        [NotNull] private readonly CommandResult commandResult;

        public CommandResultAdapter([NotNull] CommandResult commandResult)
        {
            this.commandResult = commandResult ?? throw new ArgumentNullException(nameof(commandResult));
        }

        public int ExitCode => commandResult.ExitCode;

        public bool Success => commandResult.Success;

        public string StandardOutput => commandResult.StandardOutput;

        public string StandardError => commandResult.StandardError;
    }
}
