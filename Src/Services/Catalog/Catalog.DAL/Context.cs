using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using ROL.Services.Catalog.DAL.EntityConfigurations;
using ROL.Services.Catalog.Domain;
using System;
using System.Reflection;

namespace ROL.Services.Catalog.DAL
{
	public class Context : DbContext
	{
		public DbSet<Brand> Brands { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Item> Items { get; set; }
		public DbSet<Unit> Units { get; set; }
		public DbSet<Vendor> Vendors { get; set; }
		public DbSet<Variant> Variants { get; set; }

		public DbSet<ItemCategory> ItemCategories { get; set; }

		public Context()
		{
			this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		public Context(DbContextOptions<Context> options) : base(options)
		{
			this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		public void Migrate()
		{
			if (!this.Database.IsInMemory())
			{
				this.Database.Migrate();

			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.ConfigureSqlServer("Server=(localdb)\\MSSQLLocalDB;Initial Catalog=CatalogDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
			}
		}


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfiguration(new BrandEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new CategoryEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new ItemEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new UnitEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new VariantEntityTypeConfiguration());
			modelBuilder.ApplyConfiguration(new VendorEntityTypeConfiguration());

			modelBuilder.Entity<ItemCategory>()
				.HasKey(x => new { x.ItemId, x.CategoryId });
		}

		public class ContextDesignFactory : IDesignTimeDbContextFactory<Context>
		{
			public Context CreateDbContext(string[] args)
			{
				DbContextOptionsBuilder<Context> optionsBuilder = new DbContextOptionsBuilder<Context>()
					.ConfigureSqlServer("Server=(localdb)\\MSSQLLocalDB;Initial Catalog=Catalog.API;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

				return new Context(optionsBuilder.Options);
			}

			public static Context CreateDbContext(IConfiguration configuration)
			{
				DbContextOptionsBuilder<Context> optionsBuilder = new DbContextOptionsBuilder<Context>().ConfigureFromSettings(configuration);

				return new Context(optionsBuilder.Options);
			}
		}

	}


	public static class OptionBuilderExt
	{
		public static DbContextOptionsBuilder ConfigureFromSettings(this DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
		{
			bool.TryParse(configuration["InMemoryDB"], out bool inMemory);
			if (inMemory)
			{
				optionsBuilder.ConfigureInMemory(configuration["ConnectionString"]);
			}
			else
			{
				optionsBuilder.ConfigureSqlServer(configuration["ConnectionString"]);
			}
			return optionsBuilder;
		}

		public static DbContextOptionsBuilder<Context> ConfigureFromSettings(this DbContextOptionsBuilder<Context> optionsBuilder, IConfiguration configuration)
		{
			bool.TryParse(configuration["InMemoryDB"], out bool inMemory);
			if (inMemory)
			{
				optionsBuilder.ConfigureInMemory(configuration["ConnectionString"]);
			}
			else
			{
				optionsBuilder.ConfigureSqlServer(configuration["ConnectionString"]);
			}
			return optionsBuilder;
		}

		public static DbContextOptionsBuilder<Context> ConfigureInMemory(this DbContextOptionsBuilder<Context> optionsBuilder, string dbName)
		{
			optionsBuilder.UseInMemoryDatabase<Context>(dbName)._ConfigureCommon();
			return optionsBuilder;
		}

		public static DbContextOptionsBuilder ConfigureInMemory(this DbContextOptionsBuilder optionsBuilder, string dbName)
		{
			optionsBuilder.UseInMemoryDatabase(dbName)._ConfigureCommon();
			return optionsBuilder;
		}

		public static DbContextOptionsBuilder<Context> ConfigureSqlServer(this DbContextOptionsBuilder<Context> optionsBuilder, string connection)
		{
			optionsBuilder.UseSqlServer<Context>(
				connection,
				sqlServerOptionsAction: sqlOptions =>
				{
					sqlOptions.MigrationsAssembly(typeof(Context).GetTypeInfo().Assembly.GetName().Name);
					//Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
					sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
				}
			)._ConfigureCommon();
			return optionsBuilder;
		}

		public static DbContextOptionsBuilder ConfigureSqlServer(this DbContextOptionsBuilder optionsBuilder, string connection)
		{
			optionsBuilder.UseSqlServer(
				connection,
				sqlServerOptionsAction: sqlOptions =>
				{
					sqlOptions.MigrationsAssembly(typeof(Context).GetTypeInfo().Assembly.GetName().Name);
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
