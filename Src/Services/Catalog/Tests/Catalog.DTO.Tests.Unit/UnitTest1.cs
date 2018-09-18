using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ROL.Services.Catalog.DAL;
using ROL.Services.Catalog.Domain;
using System;
using System.Linq;
using Xunit;

namespace Catalog.DTO.Tests.Unit
{
	public class UnitTest1
	{
		private IConfiguration config { get; set; }
		public UnitTest1()
		{
			config = new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
						.Build();
		}

		[Fact]
		public async void Test1()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(config))
			{
				LoggerFactory loggerFactory = new LoggerFactory();
				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				Item item = await context.Items.Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				Assert.Empty(item.MetaData);
				item.MetaData.Add("Test1", "1");
				item.MetaData.Add("Test2", "2");
				item.MetaData.Add("Test3", "3");
				context.Entry(item).State = EntityState.Modified;
				await context.SaveChangesAsync();
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(config))
			{
				Item item2 = await context2.Items.Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				Assert.Equal("1", item2.MetaData["Test1"]);
				Assert.Equal("2", item2.MetaData["Test2"]);
				Assert.Equal("3", item2.MetaData["Test3"]);

				await context2.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void Test2()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(config))
			{
				LoggerFactory loggerFactory = new LoggerFactory();
				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				Item item = await context.Items.Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				Assert.Empty(item.MetaData);
				item.Name += " - updated";
				int changes = await context.SaveChangesAsync();
				Assert.Equal(1, changes);
			}
			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(config))
			{
				Item item2 = await context2.Items.Where(i => i.Name == "5-MTHF - updated").FirstOrDefaultAsync();
				Assert.NotNull(item2);
				await context2.Database.EnsureDeletedAsync();
			}
		}
	}
}
