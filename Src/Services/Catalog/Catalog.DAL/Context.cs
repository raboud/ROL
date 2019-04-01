using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ROL.Services.Catalog.DAL.EntityConfigurations;
using ROL.Services.Catalog.Domain;

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
			ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		public Context(DbContextOptions options) : base(options)
		{
			ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

//		public Context(DbContextOptions<Context> options) : base(options)
//		{
//			ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
//		}

		public void Migrate()
		{
			if (!Database.IsInMemory())
			{
				Database.Migrate();

			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.ConfigureSqlServer<Context>("Server=(localdb)\\MSSQLLocalDB;Initial Catalog=CatalogDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
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
				DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder()
					.ConfigureSqlServer<Context>("Server=(localdb)\\MSSQLLocalDB;Initial Catalog=Catalog.API;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

				return new Context(optionsBuilder.Options);
			}

			public static Context CreateDbContext(IConfiguration configuration)
			{
				DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder<Context>().ConfigureFromSettings<Context>(configuration);

				return new Context(optionsBuilder.Options);
			}
		}

	}
}
