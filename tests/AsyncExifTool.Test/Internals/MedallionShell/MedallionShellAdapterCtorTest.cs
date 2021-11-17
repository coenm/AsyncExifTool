namespace CoenM.ExifToolLibTest.Internals.MedallionShell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using CoenM.ExifToolLib.Internals.MedallionShell;
    using CoenM.ExifToolLib.Internals.Stream;
    using FluentAssertions;
    using Xunit;

    public class MedallionShellAdapterCtorTest
    {
        private readonly Stream _stream;
        private readonly string _executable;
        private readonly List<string> _defaultArgs;

        public MedallionShellAdapterCtorTest()
        {
            _executable = "git";

            _defaultArgs = new List<string>
                                    {
                                        "-",
                                    };

            _stream = new WriteDelegatedDummyStream(new ExifToolStdOutWriter(Encoding.UTF8));
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Improve readability test")]
        public void Ctor_ShouldThrow_WhenExecutableArgumentIsNull()
        {
            // arrange
            string invalidExecutable = null;

            // act
            Action act = () => _ = new MedallionShellAdapter(
                invalidExecutable,
                _defaultArgs,
                _stream,
                null);

            // assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenExecutableArgumentIsWhitespaceString()
        {
            // arrange
            string invalidExecutable = "  ";

            // act
            Action act = () => _ = new MedallionShellAdapter(
                                                             invalidExecutable,
                                                             _defaultArgs,
                                                             _stream,
                                                             null);

            // assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenOutputStreamArgumentIsNull()
        {
            // arrange
            Stream invalidStream = null;

            // act
            Action act = () => _ = new MedallionShellAdapter(
                _executable,
                _defaultArgs,
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
                _executable,
                nullArgs,
                _stream,
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
                _executable,
                new List<string>
                {
                    "bla",
                    "RUBbi$h",
                },
                _stream,
                errorStream);

            // assert
            sut.Should().NotBeNull();
        }
    }
}
