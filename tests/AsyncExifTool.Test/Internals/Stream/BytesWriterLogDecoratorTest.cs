namespace CoenM.ExifToolLibTest.Internals.Stream
{
    using CoenM.ExifToolLib.Internals.Stream;
    using CoenM.ExifToolLib.Logging;
    using FakeItEasy;
    using Xunit;

    public class BytesWriterLogDecoratorTest
    {
        private readonly IBytesWriter decoratee;
        private readonly ILogger log;

        public BytesWriterLogDecoratorTest()
        {
            log = A.Fake<ILogger>();
            decoratee = A.Fake<IBytesWriter>();
        }

        [Fact]
        public void Write_ShouldForwardCallToDecoratee()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(decoratee, log, "pref-test-ix");
            var buffer = new byte[15];

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => decoratee.Write(buffer, 0, 10)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Write_ShouldCheckIfLoggingIsEnabledBeforeWritingToDecoratee()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(decoratee, log, "pref-test-ix");
            var buffer = new byte[15];

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => decoratee.Write(buffer, 0, 10)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void Write_ShouldLogBeforeCallingDecoratee_WhenLoggingIsEnabled()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(decoratee, log, "pref-test-ix");
            var buffer = new byte[15];
            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).Returns(true);

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => log.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => decoratee.Write(buffer, 0, 10)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void Write_ShouldNotLog_WhenLoggingIsDisabled()
        {
            // arrange
            var sut = new BytesWriterLogDecorator(decoratee, log, "pref-test-ix");
            var buffer = new byte[15];
            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).Returns(false);

            // act
            sut.Write(buffer, 0, 10);

            // assert
            A.CallTo(() => log.Log(A<LogEntry>._)).MustNotHaveHappened();
        }
    }
}
