namespace TestHelper
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public static class TestEnvironment
    {
        private const string SOLUTION_FILE_NAME = "AsyncExifTool.sln";
        private static readonly Lazy<string> _lazySolutionDirectoryFullPath = new Lazy<string>(GetSolutionDirectoryFullPathImpl);
        private static readonly Lazy<bool> _runsOnContinuousIntegration = new Lazy<bool>(IsContinuousIntegrationImpl);
        private static readonly Lazy<bool> _runsOnContinuousIntegrationTravis = new Lazy<bool>(IsRunningOnTravisImpl);
        private static readonly Lazy<bool> _runsOnContinuousIntegrationAppVeyor = new Lazy<bool>(IsRunningOnAppVeyorImpl);
        private static readonly Lazy<bool> _runsOnContinuousIntegrationDevOps = new Lazy<bool>(IsRunningOnDevOpsImpl);

        /// <summary>
        /// Gets a value indicating whether test execution runs on CI.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static bool RunsOnCI => _runsOnContinuousIntegration.Value;

        public static bool RunsOnTravis => _runsOnContinuousIntegrationTravis.Value;

        public static bool RunsOnAppVeyor => _runsOnContinuousIntegrationAppVeyor.Value;

        public static bool RunsOnDevOps => _runsOnContinuousIntegrationDevOps.Value;

        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private static string SolutionDirectoryFullPath => _lazySolutionDirectoryFullPath.Value;

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
            return bool.TryParse(Environment.GetEnvironmentVariable("System.TeamFoundationCollectionUri"), out var value) && value;
        }

        private static bool IsRunningOnTravisImpl()
        {
            return bool.TryParse(Environment.GetEnvironmentVariable("TRAVIS"), out var value) && value;
        }

        private static string GetSolutionDirectoryFullPathImpl()
        {
            static string GetRecursive(DirectoryInfo directory)
            {
                if (directory == null)
                {
                    throw new Exception($"directory cannot be null!");
                }

                while (!directory.EnumerateFiles(SOLUTION_FILE_NAME).Any())
                {
                    try
                    {
                        directory = directory.Parent;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            $"Unable to find solution directory from '{directory?.Name}' because of {ex.GetType().Name}!",
                            ex);
                    }

                    if (directory == null)
                    {
                        throw new Exception($"Unable to find solution directory from '{directory?.Name}'!");
                    }
                }

                return directory.FullName;
            }

            try
            {
                var assemblyLocation = typeof(TestEnvironment).GetTypeInfo().Assembly.Location;
                var assemblyFile = new FileInfo(assemblyLocation);
                DirectoryInfo directory = assemblyFile.Directory;
                return GetRecursive(directory);
            }
            catch
            {
                // try get DevOps repo directory
                var devOpsRepoDir = Environment.GetEnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY");
                if (string.IsNullOrWhiteSpace(devOpsRepoDir))
                {
                    throw new Exception("System.DefaultWorkingDirectory was null or empty.");
                }

                DirectoryInfo directoryInfo;
                try
                {
                    directoryInfo = new DirectoryInfo(devOpsRepoDir);
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not create DirectoryInfo from '{devOpsRepoDir}'. {e.Message}.");
                }

                try
                {
                    return GetRecursive(directoryInfo);
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not find directory with directory {directoryInfo.FullName} created from '{devOpsRepoDir}'. {e.Message}");
                }
            }
        }
    }
}
