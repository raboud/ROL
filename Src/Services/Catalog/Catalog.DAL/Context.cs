using ROL.Services.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ROL.Services.Catalog.DAL.EntityConfigurations;

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

		public Context(DbContextOptions<Context> options) : base(options)
		{
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
		}

	}
}
