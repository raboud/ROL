using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ROL.Services.Catalog.DAL;
using ROL.Services.Catalog.Domain;
using ROL.Services.Catalog.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Catalog.DTO.Tests.Unit
{
	public class DALTests : IClassFixture<DatabaseFixture>
	{
		private IConfiguration config { get { return this.fixture.config; } }
		DatabaseFixture fixture;

		public DALTests(DatabaseFixture fixture)
		{
			this.fixture = fixture;
		}

		[Fact]
		public async void MetaDataTest()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(config))
			{
				LoggerFactory loggerFactory = new LoggerFactory();
				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				Item item = await context.Items.AsTracking()
					.Include(i => i.Brand)
					.Include(i => i.ItemCategories)
						.ThenInclude(t => t.Category)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Vendor)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Unit)
					.Where(i => i.Name == "5-MTHF")
					.FirstOrDefaultAsync();
				Assert.Empty(item.MetaData);
				item.MetaData.Add("Test1", "1");
				item.MetaData.Add("Test2", "2");
				item.MetaData.Add("Test3", "3");
				context.Entry(item).State = EntityState.Modified;
				int changes = await context.SaveChangesAsync();
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(config))
			{
				Item item2 = await context2.Items.Include(i => i.Brand)
					.Include(i => i.ItemCategories)
						.ThenInclude(t => t.Category)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Vendor)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Unit)
					.Where(i => i.Name == "5-MTHF")
					.FirstOrDefaultAsync();
				Assert.Equal("1", item2.MetaData["Test1"]);
				Assert.Equal("2", item2.MetaData["Test2"]);
				Assert.Equal("3", item2.MetaData["Test3"]);

				await context2.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void NoTrackingTest()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(config))
			{
				LoggerFactory loggerFactory = new LoggerFactory();
				ILogger<ContextSeed> logger = loggerFactory.AddDebug().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				Item item = await context.Items.AsTracking().Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				Assert.Empty(item.MetaData);
				item.Name += " - updated";

				context.Items.Update(item);
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

		//[Fact]
		//public async void MapperTest()
		//{
		//	using (Context context = Context.ContextDesignFactory.CreateDbContext(config))
		//	{
		//		LoggerFactory loggerFactory = new LoggerFactory();
		//		ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();
		//		ContextSeed seed = new ContextSeed();
		//		await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

		//		var mockMapper = new MapperConfiguration(cfg =>
		//		{
		//			cfg.AddProfile(new AutoMapperProfile());
		//		});

		//		var mapper = mockMapper.CreateMapper();

		//		IQueryable<Item> query;
		//		query = context.Items
		//			.Include(i => i.Brand)
		//			.Include(i => i.ItemCategories)
		//				.ThenInclude(t => t.Category)
		//			.Include(i => i.Variants)
		//				.ThenInclude(v => v.Vendor)
		//			.Include(i => i.Variants)
		//				.ThenInclude(v => v.Unit)
		//			.OrderBy(b => b.Name);
		//			List<Item> items = await query
		//			.ToListAsync();

		//		List<ItemDTO> dto = mapper.Map<List<ItemDTO>>(items);

		//		List<Item> itmes2 = mapper.Map<List<Item>>(dto);
		//	}
		//}
	}

	public class DatabaseFixture : IDisposable
	{
		public Context dbContext { get; set; }
		public IConfiguration config { get; set; }
		public IContainer Container { get; set; }

		private const string TestSuffixConvention = "Tests";

		public DatabaseFixture()
		{
			config = new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
						.Build();
			dbContext = Context.ContextDesignFactory.CreateDbContext(config);
			LoggerFactory loggerFactory = new LoggerFactory();
			ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

			ContextSeed seed = new ContextSeed();

			seed.SeedAsync(dbContext, logger, false, Environment.CurrentDirectory, "").Wait();

			var builder = new ContainerBuilder();

			// configure your container
			// e.g. builder.RegisterModule<TestOverrideModule>();

			builder.Register(context => new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
						.Build())
				.As<IConfiguration>()
				.SingleInstance();
			builder.Register(context => dbContext)
				.As<Context>()
				.SingleInstance();

			Container = builder.Build();
		}

		public void Dispose()
		{
			Container.Dispose();
			dbContext.Dispose();
		}

	}

	public class Foo
	{
		public Foo(IConfiguration config)
		{

		}
	}

}
