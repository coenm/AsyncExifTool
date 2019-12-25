namespace ExifToolAsyncTest.Internals.MedallionShell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using ExifToolAsync.Internals.MedallionShell;
    using ExifToolAsync.Internals.Stream;
    using FluentAssertions;
    using Xunit;

    public class MedallionShellAdapterCtorTest
    {
        private const int FallbackTestTimeout = 5000;
        private readonly ExifToolStayOpenStream stream;
        private readonly string executable;
        private readonly List<string> defaultArgs;

        public MedallionShellAdapterCtorTest()
        {
            executable = "git";

            defaultArgs = new List<string>
                                    {
                                        "-",
                                    };

            stream = new ExifToolStayOpenStream(Encoding.UTF8);
        }

        [Theory]
        [InlineData("   ")]
        [InlineData(null)]
        public void Ctor_ShouldThrow_WhenExecutableArgumentIsInvalid(string invalidExecutable)
        {
            // arrange

            // act
            Action act = () => _ = new MedallionShellAdapter(
                invalidExecutable,
                defaultArgs,
                stream,
                null);

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenOutputStreamArgumentIsNull()
        {
            // arrange
            Stream invalidStream = null;

            // act
            Action act = () => _ = new MedallionShellAdapter(
                executable,
                defaultArgs,
                invalidStream,
                null);

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        // This test only works if git is a valid executable.
        [Fact]
        public void Ctor_ShouldNotThrow_WhenArgsIsNull()
        {
            // arrange
            IEnumerable<string> nullArgs = null;

            // act
            using var sut = new MedallionShellAdapter(
                executable,
                nullArgs,
                stream,
                null);

            // assert
            sut.Should().NotBeNull();
        }

        // This test only works if git is a valid executable.
        [Fact]
        public void Ctor_ShouldNotThrow_WhenErrorStreamIsUsed()
        {
            // arrange
            using var errorStream = new MemoryStream();

            // act
            using var sut = new MedallionShellAdapter(
                executable,
                new List<string>
                {
                    "bla",
                    "RUBbi$h",
                }, 
                stream,
                errorStream);

            // assert
            sut.Should().NotBeNull();
        }
    }
}
