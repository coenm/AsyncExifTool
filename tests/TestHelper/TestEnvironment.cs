namespace TestHelper
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public static class TestEnvironment
    {
        private const string SolutionFileName = "AsyncExifTool.sln";
        private static readonly Lazy<string> LazySolutionDirectoryFullPath = new Lazy<string>(GetSolutionDirectoryFullPathImpl);
        private static readonly Lazy<bool> RunsOnContinuousIntegration = new Lazy<bool>(IsContinuousIntegrationImpl);
        private static readonly Lazy<bool> RunsOnContinuousIntegrationTravis = new Lazy<bool>(IsRunningOnTravisImpl);
        private static readonly Lazy<bool> RunsOnContinuousIntegrationAppVeyor = new Lazy<bool>(IsRunningOnAppVeyorImpl);
        private static readonly Lazy<bool> RunsOnContinuousIntegrationDevOps = new Lazy<bool>(IsRunningOnDevOpsImpl);

        /// <summary>
        /// Gets a value indicating whether test execution runs on CI.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static bool RunsOnCI => RunsOnContinuousIntegration.Value;

        public static bool RunsOnTravis => RunsOnContinuousIntegrationTravis.Value;

        public static bool RunsOnAppVeyor => RunsOnContinuousIntegrationAppVeyor.Value;

        public static bool RunsOnDevOps => RunsOnContinuousIntegrationDevOps.Value;

        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private static string SolutionDirectoryFullPath => LazySolutionDirectoryFullPath.Value;

        /// <summary>
        /// Convert relative path to full path based on solution directory.
        /// </summary>
        /// <param name="relativePath">relative path from root directory.</param>
        /// <returns>Full path.</returns>
        public static string GetFullPath(params string[] relativePath)
        {
            var paths = new[] { SolutionDirectoryFullPath }.Concat(relativePath).ToArray();
            return Path
                   .Combine(paths)
                   .Replace('\\', Path.DirectorySeparatorChar);
        }

        private static bool IsContinuousIntegrationImpl()
        {
            return bool.TryParse(Environment.GetEnvironmentVariable("CI"), out var isCi) && isCi;
        }

        private static bool IsRunningOnAppVeyorImpl()
        {
            return bool.TryParse(Environment.GetEnvironmentVariable("APPVEYOR"), out var value) && value;
        }

        private static bool IsRunningOnDevOpsImpl()
        {
            // not sure what env variable to use for detection.
            return IsWindows && bool.TryParse(Environment.GetEnvironmentVariable("System.TeamFoundationCollectionUri"), out var value) && value;
        }

        private static bool IsRunningOnTravisImpl()
        {
            return bool.TryParse(Environment.GetEnvironmentVariable("TRAVIS"), out var value) && value;
        }

        private static string GetSolutionDirectoryFullPathImpl()
        {
            var assemblyLocation = typeof(TestEnvironment).GetTypeInfo().Assembly.Location;

            var assemblyFile = new FileInfo(assemblyLocation);

            var directory = assemblyFile.Directory;

            if (directory == null)
                throw new Exception($"Unable to find solution directory from '{assemblyLocation}'!");

            while (!directory.EnumerateFiles(SolutionFileName).Any())
            {
                try
                {
                    directory = directory.Parent;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to find solution directory from '{assemblyLocation}' because of {ex.GetType().Name}!", ex);
                }

                if (directory == null)
                    throw new Exception($"Unable to find solution directory from '{assemblyLocation}'!");
            }

            return directory.FullName;
        }
    }
}
