namespace CoenM.ExifToolLibTest.Internals.Stream
{
    using CoenM.ExifToolLib.Internals.Stream;
    using CoenM.ExifToolLib.Logging;
    using FakeItEasy;
    using Xunit;

    public class BytesWriterLogDecoratorTest
    {
        private readonly IBytesWriter _decoratee;
        private readonly ILogger _log;

        public BytesWriterLogDecoratorTest()
        {
            _log = A.Fake<ILogger>();
            _decoratee = A.Fake<IBytesWriter>();
        }

        [Fact]
        public void Write_ShouldForwardCallToDecoratee()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(_decoratee, _log, "pref-test-ix");
            var buffer = new byte[15];

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => _decoratee.Write(buffer, 0, 10)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Write_ShouldCheckIfLoggingIsEnabledBeforeWritingToDecoratee()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(_decoratee, _log, "pref-test-ix");
            var buffer = new byte[15];

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => _log.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => _decoratee.Write(buffer, 0, 10)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void Write_ShouldLogBeforeCallingDecoratee_WhenLoggingIsEnabled()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(_decoratee, _log, "pref-test-ix");
            var buffer = new byte[15];
            A.CallTo(() => _log.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => _log.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => _log.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => _decoratee.Write(buffer, 0, 10)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void Write_ShouldNotLog_WhenLoggingIsDisabled()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(_decoratee, _log, "pref-test-ix");
            var buffer = new byte[15];
            A.CallTo(() => _log.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => _log.Log(A<LogEntry>._)).MustNotHaveHappened();
        }
    }
}
