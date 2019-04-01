using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace ROL.Services.Catalog.DAL
{
	public static class OptionBuilderExt
	{
		public static DbContextOptionsBuilder ConfigureFromSettings<T>(this DbContextOptionsBuilder optionsBuilder, IConfiguration configuration) where T : DbContext
		{
			bool.TryParse(configuration["InMemoryDB"], out bool inMemory);
			if (inMemory)
			{
				optionsBuilder.ConfigureInMemory<T>(configuration["ConnectionString"]);
			}
			else
			{
				optionsBuilder.ConfigureSqlServer<T>(configuration["ConnectionString"]);
			}
			return optionsBuilder;
		}


		public static DbContextOptionsBuilder ConfigureInMemory<T>(this DbContextOptionsBuilder optionsBuilder, string dbName)
		{
			optionsBuilder.UseInMemoryDatabase(dbName)._ConfigureCommon();
			return optionsBuilder;
		}

		public static DbContextOptionsBuilder ConfigureSqlServer<T>(this DbContextOptionsBuilder optionsBuilder, string connection)
		{
			optionsBuilder.UseSqlServer(
				connection,
				sqlServerOptionsAction: sqlOptions =>
				{
					sqlOptions.MigrationsAssembly(typeof(T).GetTypeInfo().Assembly.GetName().Name);
					//Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
					sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
				}
			)._ConfigureCommon();
			return optionsBuilder;
		}

		private static void _ConfigureSqlServer(DbContextOptionsBuilder optionsBuilder, string connection)
		{
			optionsBuilder._ConfigureCommon();
		}

		private static void _ConfigureCommon(this DbContextOptionsBuilder optionsBuilder)
		{
			// Changing default behavior when client evaluation occurs to throw. 
			// Default in EF Core would be to log a warning when client evaluation is performed.
			optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
			//Check Client vs. Server evaluation: https://docs.microsoft.com/en-us/ef/core/querying/client-eval

		}

	}
}
