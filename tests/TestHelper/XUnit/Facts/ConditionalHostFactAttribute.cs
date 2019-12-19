namespace EagleEye.TestHelper.XUnit.Facts
{
    using Xunit;

    /// <summary>
    /// Conditional Fact attribute. Test is only executed when ran (or skipped based on TestHostMode) on given TestHost.
    /// </summary>
    public sealed class ConditionalHostFactAttribute : FactAttribute
    {
        [JetBrains.Annotations.PublicAPI]
        public ConditionalHostFactAttribute(TestHost allowedHosts, string reason = null)
        {
            if (TestEnvironment.RunsOnAppVeyor && (allowedHosts & TestHost.AppVeyorWindows) == TestHost.AppVeyorWindows)
                return;

            if (TestEnvironment.RunsOnTravis && (allowedHosts & TestHost.Travis) == TestHost.Travis)
                return;

            if (!TestEnvironment.RunsOnCI && (allowedHosts & TestHost.Local) == TestHost.Local)
                return;

            Skip = $"Test skipped. Configured to run on {allowedHosts}. " + reason;
        }

        [JetBrains.Annotations.PublicAPI]
        public ConditionalHostFactAttribute(TestHostMode mode, TestHost hosts, string reason = null)
        {
            if (TestEnvironment.RunsOnAppVeyor && (hosts & TestHost.AppVeyorWindows) == TestHost.AppVeyorWindows)
            {
                ActOnHostMatched(mode, TestHost.AppVeyorWindows, reason);
                return;
            }

            if (TestEnvironment.RunsOnTravis && (hosts & TestHost.Travis) == TestHost.Travis)
            {
                ActOnHostMatched(mode, TestHost.Travis, reason);
                return;
            }

            if (!TestEnvironment.RunsOnCI && (hosts & TestHost.Local) == TestHost.Local)
            {
                ActOnHostMatched(mode, TestHost.Local, reason);
                return;
            }

            if (mode == TestHostMode.Allow)
                Skip = $"Test skipped. Configured to run on {hosts}. " + reason;
        }

        private void ActOnHostMatched(TestHostMode mode, TestHost matchedTesthost, string reason)
        {
            if (mode == TestHostMode.Skip)
                Skip = $"Test skipped for {matchedTesthost}. " + reason;
        }
    }
}
