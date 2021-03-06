﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Microsoft.AspNetCore.Hosting
{
	public static class IWebHostExtensions
	{
		public static IWebHost MigrateDbContext<TContext>(this IWebHost webHost, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
		{
			using (IServiceScope scope = webHost.Services.CreateScope())
			{
				IServiceProvider services = scope.ServiceProvider;

				ILogger<TContext> logger = services.GetRequiredService<ILogger<TContext>>();

				TContext context = services.GetService<TContext>();

				try
				{
					logger.LogInformation($"Migrating database associated with context {typeof(TContext).Name}");

					RetryPolicy retry = Policy.Handle<SqlException>()
						 .WaitAndRetry(new TimeSpan[]
						 {
							 TimeSpan.FromSeconds(5),
							 TimeSpan.FromSeconds(10),
							 TimeSpan.FromSeconds(15),
						 });

					retry.Execute(() =>
					{
						//if the sql server container is not created on run docker compose this
						//migration can't fail for network related exception. The retry options for DbContext only 
						//apply to transient exceptions.
						if (!context.Database.IsInMemory())
						{
							context.Database.Migrate();
						}
						seeder(context, services);
					});


					logger.LogInformation($"Migrated database associated with context {typeof(TContext).Name}");
				}
				catch (Exception ex)
				{
					logger.LogError(ex, $"An error occurred while migrating the database used on context {typeof(TContext).Name}");
				}
			}

			return webHost;
		}
	}
}
