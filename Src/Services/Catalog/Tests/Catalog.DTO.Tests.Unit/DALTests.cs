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
using System.IO;
using System.Linq;
using Xunit;

namespace Catalog.DTO.Tests.Unit
{
	public class DALTests : IClassFixture<DatabaseFixture>
	{
		private IConfiguration _config => fixture.Config;

		private DatabaseFixture fixture;

		public DALTests(DatabaseFixture fixture)
		{
			this.fixture = fixture;
		}

		[Fact]
		public async void Speed1()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				//				LoggerFactory loggerFactory = new LoggerFactory();
				//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

			}
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
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
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				//				LoggerFactory loggerFactory = new LoggerFactory();
				//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

			}
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
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
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				//				LoggerFactory loggerFactory = new LoggerFactory();
				//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

			}
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
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
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				//				LoggerFactory loggerFactory = new LoggerFactory();
				//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

			}
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
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
//				context.ChangeTracker.DetectChanges();
				//				context.Items.Update(item);
				Assert.True(context.Entry(item).Property(p => p.MetaData).IsModified, "Property is modified");
				Assert.Equal(EntityState.Modified, context.Entry(item).State);
				int changes = await context.SaveChangesAsync();
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(_config))
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
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				//				LoggerFactory loggerFactory = new LoggerFactory();
				//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

			}
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
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
				context.ChangeTracker.DetectChanges();
				Assert.Equal(EntityState.Modified, context.Entry(item).State);
				int changes = await context.SaveChangesAsync();
				Assert.Equal(1, changes);
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}

			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(_config))
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
		public async void NoTrackingTest()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				//				LoggerFactory loggerFactory = new LoggerFactory();
				//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				Item item = await context.Items.AsTracking().Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				Assert.Empty(item.MetaData);
				item.Name += " - updated";

				context.Items.Update(item);
				Assert.Equal(EntityState.Modified, context.Entry(item).State);

				int changes = await context.SaveChangesAsync();
				Assert.Equal(1, changes);
				Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
			}
			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				Item item2 = await context2.Items.Where(i => i.Name == "5-MTHF - updated").FirstOrDefaultAsync();
				Assert.NotNull(item2);
				await context2.Database.EnsureDeletedAsync();
			}
		}

		[Fact]
		public async void NoTrackingTest2()
		{
			using (Context context = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
				//				LoggerFactory loggerFactory = new LoggerFactory();
				//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

				ContextSeed seed = new ContextSeed();

				await seed.SeedAsync(context, logger, false, Environment.CurrentDirectory, "");

				Item item = await context.Items.Where(i => i.Name == "5-MTHF").FirstOrDefaultAsync();
				//				Assert.Empty(item.MetaData);
				item.Name += " - updated2";

				context.Entry(item).State = EntityState.Detached;
				Assert.Throws<InvalidOperationException>(() => context.Items.Update(item));
				int changes = await context.SaveChangesAsync();
				Assert.Equal(0, changes);
			}
			using (Context context2 = Context.ContextDesignFactory.CreateDbContext(_config))
			{
				Item item2 = await context2.Items.Where(i => i.Name == "5-MTHF - updated2").FirstOrDefaultAsync();
				Assert.Null(item2);
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
//			.ToListAsync();

			ItemDTO dto = mapper.Map<ItemDTO>(items);

			Item items2 = mapper.Map<Item>(dto);

			string expected = JsonConvert.SerializeObject(items, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling  = NullValueHandling.Ignore});
			string actual = JsonConvert.SerializeObject(items2, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
			Assert.Equal(expected, actual);
		}
	}

	public class DatabaseFixture : IDisposable
	{
		public ServiceProvider serviceProvider { get; set; }

		public Context Context { get; set; }
		public IConfiguration Config { get; set; }
		//		public IContainer Container { get; set; }

		private const string TestSuffixConvention = "Tests";

		public DatabaseFixture()
		{
			ServiceCollection serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			this.serviceProvider = serviceCollection.BuildServiceProvider();

			Config = new ConfigurationBuilder()
						  .SetBasePath(Directory.GetCurrentDirectory())
						  .AddJsonFile("appsettings.json", false)
						  .Build();

			Context = this.serviceProvider.GetService<Context>(); // Context.ContextDesignFactory.CreateDbContext(Config);
			ILogger<ContextSeed> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ContextSeed>();
			//				LoggerFactory loggerFactory = new LoggerFactory();
			//				ILogger<ContextSeed> logger = loggerFactory.AddConsole().CreateLogger<ContextSeed>();

			ContextSeed seed = new ContextSeed();

			seed.SeedAsync(Context, logger, false, Environment.CurrentDirectory, "").Wait();

		}


		private void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddConsole();
				loggingBuilder.AddDebug();
			});

			// add logging
			services.AddLogging();

			// build configuration

			services.AddDbContext<Context>(options =>
			{
				options.ConfigureFromSettings<Context>(Config);
			});

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies().Where(a => (!a.FullName.StartsWith("Microsoft.")))
                .OrderByDescending(x => x.FullName));

            //ContainerBuilder container = new ContainerBuilder();
            //container.Populate(services);
            //return new AutofacServiceProvider(container.Build());



        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //			Container.Dispose();
                    Context.Dispose();
                    serviceProvider.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DatabaseFixture()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    public class Foo
	{
		public Foo(IConfiguration config)
		{

		}
	}

}
