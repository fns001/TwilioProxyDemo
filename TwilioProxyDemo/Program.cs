using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwilioProxyDemo
{
    public class Program
    {
        const string outputTemplateConsole = @"{Timestamp:HH:mm:ss} [{Level:u3}] [{SourceContext}] {NewLine}{Message:lj}{NewLine}{Exception}";
        const string outputTemplateFile = @"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
        const int logFileInterval = 3;

        public static void Main(string[] args)
        {
            ConfigureLogger();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Web host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .UseSerilog();

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: outputTemplateConsole)
            .WriteTo.Debug(outputTemplate: outputTemplateFile)
#else
            //The line below is intended for Azure hosting. Uncomment to write to a log file.
            .WriteTo.File(@"D:\home\LogFiles\proxydemolog_.txt", fileSizeLimitBytes: 10000000, rollingInterval: RollingInterval.Month, rollOnFileSizeLimit: true, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(logFileInterval), outputTemplate: outputTemplateFile)
#endif
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore"))
            .CreateLogger();
        }
    }
}
