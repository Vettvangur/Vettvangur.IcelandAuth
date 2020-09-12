using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Vettvangur.IcelandAuth.Sample.AspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var h = new WebHostBuilder();
            var environment = h.GetSetting("environment");
            Console.Title = "AspNetCore IcelandAuth";
            // We manually load the default configuration before UseNLog
            // This is needed so the logger statements below work correctly during startup
            var builder = new ConfigurationBuilder()
                .AddJsonFile(path: "AppSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(path: $"appsettings.{environment}.json", optional: true).AddEnvironmentVariables();
            var config = builder.Build();
            NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = config;

            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("Startup");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseNLog()
                ;
    }
}
