using System;

namespace Samples
{
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

            Console.WriteLine("Sample application using AsynExifTool in combination with NLog");

            // AsyncExifTool configuration. Please make sure exiftool.exe is accessible.
            var commonArgs = new List<string>();
            var asyncExifToolConfiguration = new AsyncExifToolConfiguration(
                "exiftool.exe",
                Encoding.UTF8,
                Environment.NewLine,
                commonArgs);

            // Create a logger for AsyncExifTool. AsyncExifTool does not require any logging framework. You have to write your own adapter.
            var logger = new AsyncExifToolToNLogAdapter(nlogLogger);

            // Create AsyncExifTool instance. You can also do this without a logger.
            // ie:
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
