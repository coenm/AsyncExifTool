namespace CoenM.ExifToolLibTest.Internals.MedallionShell
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using CoenM.ExifToolLib.Internals.MedallionShell;
    using FluentAssertions;
    using Medallion.Shell;
    using Xunit;

    public class CommandResultAdapterTest
    {
        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void CommandResultAdapter_Ctor_ShouldThrow_WhenArgumentIsNull()
        {
            // arrange

            // act
            Action act = () => _ = new CommandResultAdapter(null);

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void CommandResultAdapter_ShouldAdapt_WhenProcessHadError()
        {
            // arrange
            var cmd = Command.Run("git", "s");

            // act
            var sut = new CommandResultAdapter(cmd.Result);

            // assert
            sut.ExitCode.Should().Be(1);
            sut.Success.Should().BeFalse("The command 's' is not a git command");
            sut.StandardOutput.Should().BeEmpty();
            sut.StandardError.Should().StartWith("git: 's' is not a git command. See 'git --help'.");
        }

        [Fact]
        public void CommandResultAdapter_ShouldAdapt_WhenProcessSucceeded()
        {
            // arrange
            var cmd = Command.Run("git", "version");

            // act
            var sut = new CommandResultAdapter(cmd.Result);

            // assert
            sut.ExitCode.Should().Be(0);
            sut.Success.Should().BeTrue();
            sut.StandardOutput.Should().StartWith("git version"); // git version 2.24.0.windows.2
            sut.StandardError.Should().BeEmpty();
        }
    }
}
