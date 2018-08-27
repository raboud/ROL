using Catalog.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
	public class Context : DbContext
	{
		internal DbSet<Brand> Brands { get; set; }
		internal DbSet<Category> Categories { get; set; }
		internal DbSet<Item> Items { get; set; }
		internal DbSet<Unit> Units { get; set; }
		internal DbSet<Vendor> Vendors { get; set; }

		internal DbSet<ItemCategory> ItemCategories { get; set; }

		public Context(DbContextOptions<Context> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ItemCategory>()
				.HasKey(x => new { x.ItemId, x.CategoryId });

			modelBuilder.Entity<Item>(e =>
			{
				e.Property(i => i.MetaData)
					.HasConversion(
						d => JsonConvert.SerializeObject(d, Formatting.None),
						s => JsonConvert.DeserializeObject<Dictionary<string, string>>(s)
					)
					.HasMaxLength(4000)
					.IsRequired();
			});

			modelBuilder.Entity<Variant>(e =>
			{
				e.Property(i => i.MetaData)
					.HasConversion(
						d => JsonConvert.SerializeObject(d, Formatting.None),
						s => JsonConvert.DeserializeObject<Dictionary<string, string>>(s)
					)
					.HasMaxLength(4000)
					.IsRequired();
			});
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
