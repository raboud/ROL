using ROL.Services.Catalog.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ROL.Services.Catalog.DAL
{
	public class ContextSeed
	{
		public async Task SeedAsync(Context context, IHostingEnvironment env, IOptions<Settings> settings, ILogger<ContextSeed> logger)
		{
			Policy policy = CreatePolicy(logger, nameof(ContextSeed));

			await policy.ExecuteAsync(async() => 
			{
				bool useCustomizationData = settings.Value.UseCustomizationData;
				string contentRootPath = env.ContentRootPath;
				string picturePath = env.WebRootPath;

				string fileName = Path.Combine(contentRootPath, "Setup", "Catalog.json");
				if (File.Exists(fileName))
				{
//					string raw = await File.ReadAllTextAsync(fileName);
					string raw;
					using (StreamReader sourceReader = File.OpenText(fileName))
					{
						raw = await sourceReader.ReadToEndAsync();
					}
					dynamic data = JObject.Parse(raw);

					await ProcessBrands(data.Brands, context, logger);
					await ProcessVendors(data.Vendors, context, logger);
					await ProcessUnits(data.Units, context, logger);
					await ProcessCategories(data.Categories, context, logger);
					//					await processProducts(data.Products, context, logger);

					await context.SaveChangesAsync();
				}

//				GetCatalogItemPictures(contentRootPath, picturePath);
			});
		}

		private Policy CreatePolicy(ILogger<ContextSeed> logger, string prefix, int retries = 3)
		{
			return Policy.Handle<SqlException>().
				WaitAndRetryAsync(
					retryCount: retries,
					sleepDurationProvider: retry => System.TimeSpan.FromSeconds(5),
					onRetry: (exception, timeSpan, retry, ctx) =>
					{
						logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
					}
				);
		}

		static async Task ProcessBrands(dynamic items, Context context, ILogger<ContextSeed> logger)
		{
			List<Brand> data = await context.Brands.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Brands.Add(new Brand() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}

		static async Task ProcessVendors(dynamic items, Context context, ILogger<ContextSeed> logger)
		{
			List<Vendor> data = await context.Vendors.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Vendors.Add(new Vendor() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}

		static async Task ProcessUnits(dynamic items, Context context, ILogger<ContextSeed> logger)
		{
			List<Unit> data = await context.Units.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Units.Add(new Unit() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}

		static async Task ProcessCategories(dynamic items, Context context, ILogger<ContextSeed> logger)
		{
			List<Category> data = await context.Categories.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					context.Categories.Add(new Category() { Name = item });
				}
			}
			await context.SaveChangesAsync();
		}


		static async Task ProcessProducts(dynamic items, Context context, ILogger<ContextSeed> logger)
		{
			List<Brand> brands = await context.Brands.ToListAsync();
			List<Vendor> vendors = await context.Vendors.ToListAsync();
			List<Category> categories = await context.Categories.ToListAsync();
			List<Unit> units = await context.Units.ToListAsync();


			List<ItemCategory> pcs = new List<ItemCategory>();

			List<Item> data = await context.Items.ToListAsync();
			foreach (dynamic item in items)
			{
				if (!data.Any(b => b.Name == (string)item.Name)) // && b.Count == (int)item.Count))
				{
					Brand brand = brands.FirstOrDefault(b => b.Name == (string)item.Brand);
					Unit unit = units.FirstOrDefault(b => b.Name == (string)item.Unit);
					Vendor vendor = vendors.FirstOrDefault(b => b.Name == (string)item.Vendor);

					if (brand == null || unit == null || vendor == null)
					{
						string name = item.Name;
						logger.LogError("invalid item", name);
						return;
					}
					Item p = new Item
					{
						Id = item.Id,
						Name = item.Name,
						Brand = brand
					};
					p.BrandId = p.Brand.Id;
					p.Description = item.Description;
					p.PictureFileName = item.PictureFileName;

					//p.Unit = unit;
					//p.UnitId = p.Unit.Id;
					//p.Vendor = vendor;
					//p.VendorId = p.Vendor.Id;
					//p.AvailableStock = item.AvailableStock;
					//p.Cost = item.Cost;
					//p.Count = item.Count;
					//p.MaxStockThreshold = item.MaxStockThreshold;
					//p.Price = item.Price;
					//p.RestockThreshold = item.RestockThreshold;
					//p.SuggestPrice = item.SuggestPrice;

					await context.Items.AddAsync(p);
					//					await context.SaveChangesAsync();

					foreach (string category in item.Categories)
					{
						ItemCategory pc = new ItemCategory
						{
							ItemId = p.Id,
							Category = categories.FirstOrDefault(c => c.Name == category)
						};
						p.ItemCategories.Add(pc);
					}
				}
			}
			//			await context.ProductCategories.AddRangeAsync(pcs);
			await context.SaveChangesAsync();
		}

	}
}
