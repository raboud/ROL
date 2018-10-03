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
		public Task SeedAsync(Context context, IHostingEnvironment env, IOptions<Settings> settings, ILogger<ContextSeed> logger)
		{
			bool useCustomizationData = settings.Value.UseCustomizationData;
			string contentRootPath = env.ContentRootPath;
			string picturePath = env.WebRootPath;
			return SeedAsync(context, logger, useCustomizationData, contentRootPath, picturePath);
		}

		public async Task SeedAsync(Context context, ILogger<ContextSeed> logger, bool useCustomizationData, string contentRootPath, string picturePath)
		{
			logger.LogInformation("Enterring SeedAsync");

			Policy policy = CreatePolicy(logger, nameof(ContextSeed));

			await policy.ExecuteAsync(async() => 
			{
				string fileName = Path.Combine(contentRootPath, "Setup", "Catalog.json");
				if (File.Exists(fileName))
				{
					QueryTrackingBehavior initialState = context.ChangeTracker.QueryTrackingBehavior;
					context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
					
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
					await ProcessProducts(data.Products, context, logger);

					await context.SaveChangesAsync();

					context.ChangeTracker.QueryTrackingBehavior = initialState ;
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
			foreach (dynamic item in items)
			{
				if (!data.Any(b => b.Name == (string) item.Name))
				{
					Category category = new Category() { Name = item.Name };
					foreach (dynamic child in item.Children)
					{
						if (!data.Any(b => b.Name == (string) child.Name))
						{
							category.Children.Add(new Category() { Name = child.Name });
						}
					}
					context.Categories.Add(category);
				}
			}
			int changes = await context.SaveChangesAsync();
		}


		static async Task ProcessProducts(dynamic items, Context context, ILogger<ContextSeed> logger)
		{
			logger.LogInformation("Enterring ProcessProducts");

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

					if (brand == null)
					{
						string name = item.Name;
						logger.LogError("invalid item", name);
						return;
					}
					Item p = new Item
					{
//						Id = item.Id,
						Name = item.Name,
						Brand = brand
					};
					p.BrandId = p.Brand.Id;
					p.Description = item.Description;
					p.PictureFileName = item.PictureFileName;

					foreach (dynamic variant in item.Variants)
					{
						Variant v = new Variant();
						v.Item = p;
						Unit unit = units.FirstOrDefault(b => b.Name == (string)variant.Unit);
						Vendor vendor = vendors.FirstOrDefault(b => b.Name == (string)variant.Vendor);
						v.Count = variant.Count;
						//p.Unit = unit;
						v.UnitId = unit.Id;

						//p.Vendor = vendor;
						v.VendorId = vendor.Id;
						v.AvailableStock = variant.AvailableStock;
						v.Cost = variant.Cost;
						v.MaxStockThreshold = variant.MaxStockThreshold;
						v.Price = variant.Price;
						v.RestockThreshold = variant.RestockThreshold;
						v.SuggestPrice = variant.SuggestPrice;
						p.Variants.Add(v);
					}


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
