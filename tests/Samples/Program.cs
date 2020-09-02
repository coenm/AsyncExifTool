namespace Samples
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib;
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    public static class Program
    {
        public static async Task Main()
        {
            ConfigureNLog();
            var nlogLogger = LogManager.GetCurrentClassLogger(typeof(AsyncExifTool));

            Console.WriteLine("Sample application using AsyncExifTool in combination with NLog");

            #region ExifToolConfiguration
            // We need to tell AsyncExifTool where exiftool executable is located.
            var exifToolExe = @"D:\exiftool.exe";

            // The encoding AsyncExifTool should use to decode the resulting bytes
            var exifToolEncoding = Encoding.UTF8;

            // common args for each exiftool command.
            // see https://exiftool.org/exiftool_pod.html#common_args for more information.
            // can be null or empty
            var commonArgs = new List<string>
                {
                    "-common_args",
                };

            // custom exiftool configuration filename.
            // see https://exiftool.org/exiftool_pod.html#config-CFGFILE for more information.
            // make sure the filename exists.
            // it is also possible to create a configuration without a custom exiftool config.
            var customExifToolConfigFile = @"C:\AsyncExifTool.ExifTool_config";

            // Create configuration to be used in AsyncExifTool.
            var asyncExifToolConfiguration = string.IsNullOrWhiteSpace(customExifToolConfigFile)
                ? new AsyncExifToolConfiguration(exifToolExe, exifToolEncoding, commonArgs)
                : new AsyncExifToolConfiguration(exifToolExe, customExifToolConfigFile, exifToolEncoding, commonArgs);

            #endregion



            // Create a logger for AsyncExifTool. AsyncExifTool does not require any logging framework. You have to write your own adapter.
            var logger = new AsyncExifToolToNLogAdapter(nlogLogger);

            // Create AsyncExifTool instance. You can also do this without a logger.
            // await using var exiftool = new AsyncExifTool(asyncExifToolConfiguration);
            await using var exiftool = new AsyncExifTool(asyncExifToolConfiguration, logger);

            try
            {
                // initialize. At this point the exiftool process is started.
                // this call might throw an exception.
                exiftool.Initialize();
            }
            catch (AsyncExifToolInitialisationException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("press enter to exit");
                Console.ReadKey();
                return;
            }

            // Just some calls to ExifTool (using an extension method)
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(await exiftool.GetVersionAsync());
            }

            try
            {
                var result = await exiftool.ExecuteAsync("dummy command");
                Console.WriteLine("dummy command result: " + result);
            }
            catch (Exception e)
            {
                Console.WriteLine("dummy command result exception: " + e.Message);
            }

            var file = @"1.jpg";
            try
            {
                Console.WriteLine("Add person to existing image.");
                var result = await exiftool.ExecuteAsync(new[] { "-Xmp:PersonInImage+=\"Test person\"", file});
                Console.WriteLine($"RESULT: {result}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR MSG: {e.Message}");
            }

            Console.WriteLine(string.Empty);

            try
            {
                Console.WriteLine("Add person to NON existing image.");
                var result = await exiftool.ExecuteAsync(new[] { "-Xmp:PersonInImage+=\"Test person\"", file + @"xxx"});
                Console.WriteLine($"RESULT: {result}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR MSG: {e.Message}");
            }
            Console.WriteLine(string.Empty);

            try
            {
                Console.WriteLine("Get person information from image.");
                var result = await exiftool.ExecuteAsync(new[] { "-Xmp:PersonInImage", file });
                Console.WriteLine($"RESULT: {result}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR MSG: {e.Message}");
            }

            Console.WriteLine(string.Empty);

            // we are done
            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }

        private static void ConfigureNLog()
        {
            // this can also be done in config files.
            // you can use any logging framework you want
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget("target1")
                {
                    Layout = @"${date:format=HH\:mm\:ss} ${level} ${message} ${exception}"
                };
            config.AddTarget(consoleTarget);

            config.AddRuleForAllLevels(consoleTarget); // all to console

            // Step 4. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}
