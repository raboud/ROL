using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ROL.Services.Catalog.DAL;

namespace ROL.Services.Catalog.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
//			System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			CreateWebHostBuilder(args).Build()
				.MigrateDbContext<Context>((context, services) =>
				{
					IWebHostEnvironment env = services.GetService<IWebHostEnvironment>();
					IOptions<Settings> settings = services.GetService<IOptions<Settings>>();
					ILogger<ContextSeed> logger = services.GetService<ILogger<ContextSeed>>();

					new ContextSeed()
					.SeedAsync(context, env.ContentRootPath, env.WebRootPath, settings, logger)
					.Wait();

				})
//				.MigrateDbContext<IntegrationEventLogContext>((_, __) => { })
				.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				//				.UseApplicationInsights()
				//				.UseHealthChecks("/hc")
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseWebRoot("Pics")
				.ConfigureAppConfiguration((builderContext, config) =>
				{
					config.AddEnvironmentVariables();
				})
				.ConfigureLogging((hostingContext, builder) =>
				{
					builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					builder.AddConsole();
					builder.AddDebug();
				})
;

	}

	public static class Ext
	{
		public static IWebHostBuilder UseApplicationInsights(this IWebHostBuilder webHostBuilder)
		{
			return webHostBuilder;
		}

	}
}
