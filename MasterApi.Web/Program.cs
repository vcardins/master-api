using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

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
            return WebHost
                .CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(ConfigConfiguration)
				.ConfigureLogging(ConfigureLogger)
				.UseStartup<Startup>()
                .UseDefaultServiceProvider(options => options.ValidateScopes = false)
                .Build();
        }

		static void ConfigConfiguration(WebHostBuilderContext ctx, IConfigurationBuilder config)
		{
			var env = ctx.HostingEnvironment;
			config.SetBasePath(Directory.GetCurrentDirectory())
			  .AddJsonFile("appsettings.json", false, true)
			  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
			  .AddEnvironmentVariables();

			if (env.IsDevelopment())
			{
				// This reads the configuration keys from the secret store.
				// For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
				config.AddUserSecrets<Startup>();
			}
		}

		static void ConfigureLogger(WebHostBuilderContext ctx, ILoggingBuilder logging)
		{
			logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
			logging.AddConsole();
			logging.AddDebug();
		}
	}
}