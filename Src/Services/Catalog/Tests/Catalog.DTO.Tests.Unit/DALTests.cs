using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ROL.Services.Catalog.API.Infrastructure;
using ROL.Services.Catalog.DAL;
using ROL.Services.Catalog.Domain;
using ROL.Services.Catalog.DTO;
//using ROL.Services.Catalog.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Catalog.DTO.Tests.Unit
{
	public class DALTests : IClassFixture<DatabaseFixture>
	{
		private IConfiguration Config => fixture.Config;

		private readonly DatabaseFixture fixture;

		public DALTests(DatabaseFixture fixture)
		{
			this.fixture = fixture;
		}

		[Fact]
		public async void Speed1()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");
			}

			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				List<Brand> brands = await context.Brands.AsTracking().ToListAsync();
				List<Vendor> vendors = await context.Vendors.AsTracking().ToListAsync();
				List<ROL.Services.Catalog.Domain.Unit> units = await context.Units.AsTracking().ToListAsync();
				List<Category> categories = await context.Categories.AsTracking().ToListAsync();

				List<Item> item = await context.Items.AsTracking()
					.Include(i => i.ItemCategories)
					.Include(i => i.Variants)
					.ToListAsync();

				Assert.NotNull(item[0].Brand);
				Assert.NotNull(item[0].ItemCategories[0].Category);
				Assert.NotNull(item[0].Variants[0].Unit);
				Assert.NotNull(item[0].Variants[0].Vendor);

				await context.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void Speed2()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");
			}

			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				List<Item> item = await context.Items.AsTracking()
					.Include(i => i.Brand)
					.Include(i => i.ItemCategories)
						.ThenInclude(t => t.Category)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Vendor)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Unit)
					.ToListAsync();

				Assert.NotNull(item[0].Brand);
				Assert.NotNull(item[0].ItemCategories[0].Category);
				Assert.NotNull(item[0].Variants[0].Unit);
				Assert.NotNull(item[0].Variants[0].Vendor);

				await context.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void Speed3()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");
			}

			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				List<Brand> brands = await context.Brands.AsTracking().ToListAsync();
				List<Vendor> vendors = await context.Vendors.AsTracking().ToListAsync();
				List<ROL.Services.Catalog.Domain.Unit> units = await context.Units.AsTracking().ToListAsync();
				List<Category> categories = await context.Categories.AsTracking().ToListAsync();

				List<Item> item = await context.Items.AsTracking()
					.Include(i => i.ItemCategories)
					.Include(i => i.Variants)
					.ToListAsync();

				Assert.NotNull(item[0].Brand);
				Assert.NotNull(item[0].ItemCategories[0].Category);
				Assert.NotNull(item[0].Variants[0].Unit);
				Assert.NotNull(item[0].Variants[0].Vendor);

				await context.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void MetaDataTest()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");
			}

			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				List<Brand> brands = await context.Brands.AsTracking().ToListAsync();
				List<Vendor> vendors = await context.Vendors.AsTracking().ToListAsync();
				List<ROL.Services.Catalog.Domain.Unit> units = await context.Units.AsTracking().ToListAsync();
				List<Category> categories = await context.Categories.AsTracking().ToListAsync();

				Item item = await context.Items.AsTracking()
					.Include(i => i.ItemCategories)
					.Include(i => i.Variants)
					.Where(i => i.Name == "5-MTHF")
					.FirstOrDefaultAsync();
				Assert.Empty(item.MetaData);
				item.MetaData.Add("Test1", "1");
				item.MetaData.Add("Test2", "2");
				item.MetaData.Add("Test3", "3");

				Assert.True(context.Entry(item).Property(p => p.MetaData).IsModified, "Property is modified");
				Assert.Equal(EntityState.Modified, context.Entry(item).State);
				int changes = await context.SaveChangesAsync();
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				List<Brand> brands = await context2.Brands.AsTracking().ToListAsync();
				List<Vendor> vendors = await context2.Vendors.AsTracking().ToListAsync();
				List<ROL.Services.Catalog.Domain.Unit> units = await context2.Units.AsTracking().ToListAsync();
				List<Category> categories = await context2.Categories.AsTracking().ToListAsync();

				Item item2 = await context2.Items
					.Include(i => i.ItemCategories)
					.Include(i => i.Variants)
					.Where(i => i.Name == "5-MTHF")
					.FirstOrDefaultAsync();
				Assert.Equal("1", item2.MetaData["Test1"]);
				Assert.Equal("2", item2.MetaData["Test2"]);
				Assert.Equal("3", item2.MetaData["Test3"]);

				await context2.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void MetaDataTest2()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");
			}

			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{

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
				Assert.Equal(EntityState.Modified, context.Entry(item).State);
				int changes = await context.SaveChangesAsync();
				Assert.Equal(1, changes);
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				Item item2 = await context2.Items
					.Include(i => i.Brand)
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
		public async void MetaDataTest3()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");
			}

			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
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
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
				int changes = await context.SaveChangesAsync();
				Assert.Equal(0, changes);
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				Item item2 = await context2.Items
					.Include(i => i.Brand)
					.Include(i => i.ItemCategories)
						.ThenInclude(t => t.Category)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Vendor)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Unit)
					.Where(i => i.Name == "5-MTHF")
					.FirstOrDefaultAsync();
				Assert.Empty(item2.MetaData);
				await context2.Database.EnsureDeletedAsync();
			}
		}

		class person
		{
			public string FirstName { get; set; }
			public string MiddleName { get; set; }
			public string LastName { get; set; }
		}

		[Fact]
		public async void MetaDataTest4()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");
			}

			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{

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
				item.MetaData.Add("Test2", new List<string> { "Test", "this" });
				item.MetaData.Add("Test3", new person { FirstName = "Robert", LastName = "Raboud", MiddleName = "Alfred" });

				Assert.Equal(EntityState.Modified, context.Entry(item).State);
				int changes = await context.SaveChangesAsync();
				Assert.Equal(1, changes);
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				Item item2 = await context2.Items
					.Include(i => i.Brand)
					.Include(i => i.ItemCategories)
						.ThenInclude(t => t.Category)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Vendor)
					.Include(i => i.Variants)
						.ThenInclude(v => v.Unit)
					.Where(i => i.Name == "5-MTHF")
					.FirstOrDefaultAsync();
				Assert.Equal("1", item2.MetaData["Test1"]);

				List<string> list = item2.MetaData["Test2"] as List<string>;
				Assert.NotNull(list);
				Assert.NotEmpty(list);
				Assert.Equal("Test", list[0]);
				Assert.Equal("this", list[1]);

				person person = item2.MetaData["Test3"] as person;
				Assert.Equal("Robert", person.FirstName);
				Assert.Equal("Alfred", person.MiddleName);
				Assert.Equal("Raboud", person.LastName);

				await context2.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void NoTrackingTest()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				Item item = await context.Items.AsTracking().Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				Assert.Empty(item.MetaData);
				item.Name += " - updated";

				Assert.Equal(EntityState.Modified, context.Entry(item).State);

				int changes = await context.SaveChangesAsync();
				Assert.Equal(1, changes);
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				Item item2 = await context2.Items.Where(i => i.Name == "5-MTHF - updated").FirstOrDefaultAsync();
				Assert.NotNull(item2);
				await context2.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void NoTrackingTest2()
		{
			Item item = null;
			int count = 0;
			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				ContextSeed seed = new ContextSeed();
				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				item = await context.Items.Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				item.Name += " - updated2";
				//				context.Update<Item>(item);
				int changes = await context.SaveChangesAsync();
				Assert.Equal(0, changes);
				count = context.Items.Count();
			}


			using (Context context = Context.ContextDesignFactory.CreateDbContext(Config))
			{
//				item.Name += " - updated2";
				context.Update<Item>(item);
//				context.Entry(item).State = EntityState.Modified;
//				Assert.Throws<InvalidOperationException>(() => context.Items.Update(item));
				int changes = await context.SaveChangesAsync();
				Assert.Equal(1, changes);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(Config))
			{
				Item item2 = await context2.Items.Where(i => i.Name == "5-MTHF - updated2").FirstOrDefaultAsync();
				Assert.NotNull(item2);
				Assert.Equal(count, context2.Items.Count());
				await context2.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void MapperTest()
		{
			Context context = this.fixture.Context;

			IMapper mapper = this.fixture.serviceProvider.GetService<IMapper>();

			IQueryable<Item> query;
			query = context.Items
				.Include(i => i.Brand)
				.Include(i => i.ItemCategories)
					.ThenInclude(t => t.Category)
				.Include(i => i.Variants)
					.ThenInclude(v => v.Vendor)
				.Include(i => i.Variants)
					.ThenInclude(v => v.Unit)
				.OrderBy(b => b.Name);
			List<Item> items = await query
			.ToListAsync();

			List<ItemDTO> dto = mapper.Map<List<ItemDTO>>(items);
			List<Item> itmes2 = mapper.Map<List<Item>>(dto);
		}

		[Fact]
		public async void MapperTest2()
		{
			Context context = this.fixture.Context;

			IMapper mapper = this.fixture.serviceProvider.GetService<IMapper>();

			IQueryable<Item> query;
			query = context.Items
				.Include(i => i.Brand)
				.Include(i => i.ItemCategories)
					.ThenInclude(t => t.Category)
				.Include(i => i.Variants)
					.ThenInclude(v => v.Vendor)
				.Include(i => i.Variants)
					.ThenInclude(v => v.Unit)
				.OrderBy(b => b.Name);
			Item items = await query.FirstOrDefaultAsync();

			ItemDTO dto = mapper.Map<ItemDTO>(items);

			Item items2 = mapper.Map<Item>(dto);

			string expected = JsonConvert.SerializeObject(items, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling  = NullValueHandling.Ignore});
			string actual = JsonConvert.SerializeObject(items2, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
			Assert.Equal(expected, actual);
		}
	}
}
