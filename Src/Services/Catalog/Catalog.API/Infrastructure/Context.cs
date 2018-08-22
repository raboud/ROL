using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
	public class Context : DbContext
	{
		public Context(DbContextOptions<Context> options) : base(options)
		{
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
