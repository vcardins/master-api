using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace MasterApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        /// <summary>
        /// Builds the web host.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // Other Loggers.
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    // There are other logging options available:
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1
                    // logging.AddDebug();
                    // logging.AddConsole();
                })

                // Application Insights.
                // An alternative logging and metrics service for your application.
                // https://azure.microsoft.com/en-us/services/application-insights/
                // .UseApplicationInsights()
                .UseStartup<Startup>()
                .Build();
        }
    }
}