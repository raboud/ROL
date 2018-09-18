using ROL.Services.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ROL.Services.Catalog.DAL.EntityConfigurations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
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

		}

		public Context(DbContextOptions<Context> options) : base(options)
		{
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
				optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Initial Catalog=CatalogDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
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
					//                .UseSqlServer("Server=.;Initial Catalog=HMS.CatalogDb;Integrated Security=true");
					.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Initial Catalog=HMS.CatalogDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

				return new Context(optionsBuilder.Options);
			}

			static public Context CreateDbContext(IConfiguration configuration)
			{
				DbContextOptionsBuilder<Context> optionsBuilder = new DbContextOptionsBuilder<Context>();
				bool inMemory = false;
				bool.TryParse(configuration["InMemoryDB"], out inMemory);
				if (inMemory)
				{
					optionsBuilder.UseInMemoryDatabase<Context>(configuration["ConnectionString"]);
				}
				else
				{
					optionsBuilder.UseSqlServer(configuration["ConnectionString"],
										 sqlServerOptionsAction: sqlOptions =>
										 {
											 sqlOptions.MigrationsAssembly(typeof(Context).GetTypeInfo().Assembly.GetName().Name);
										 //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
										 sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
										 });
				}
				// Changing default behavior when client evaluation occurs to throw. 
				// Default in EF Core would be to log a warning when client evaluation is performed.
				optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
				//Check Client vs. Server evaluation: https://docs.microsoft.com/en-us/ef/core/querying/client-eval

				return new Context(optionsBuilder.Options);
			}
		}

	}
}
