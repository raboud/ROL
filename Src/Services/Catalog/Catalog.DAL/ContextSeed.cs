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
using Polly.Retry;

namespace ROL.Services.Catalog.DAL
{
	public class ContextSeed
	{
		public Context Context;
		public ILogger<ContextSeed> Logger;
		public string ContentRootPath { get; set; }
		public string PicturePath { get; set; }
		public bool UseCustomizationData { get; set; } = false;

		public Task SeedAsync(Context context, string contentRootPath, string webRootPath, IOptions<Settings> settings, ILogger<ContextSeed> logger)
		{
			this.Context = context;
			this.Logger = logger;
			UseCustomizationData = settings.Value.UseCustomizationData;
			ContentRootPath = contentRootPath;
			PicturePath = webRootPath;
			return SeedAsync();
		}

		public Task SeedAsync(Context context, ILogger<ContextSeed> logger, bool useCustomizationData, string contentRootPath, string picturePath)
		{
			this.Context = context;
			this.Logger = logger;
			this.UseCustomizationData = useCustomizationData;
			this.ContentRootPath = contentRootPath;
			this.PicturePath = picturePath;
			return SeedAsync();
		}

		private async Task SeedAsync()
		{
			Logger.LogInformation("Enterring SeedAsync");

			AsyncRetryPolicy policy = CreatePolicy(nameof(ContextSeed));

			await policy.ExecuteAsync(async() => 
			{
				string fileName = Path.Combine(ContentRootPath, "Setup", "Catalog.json");
				if (File.Exists(fileName))
				{
					QueryTrackingBehavior initialState = Context.ChangeTracker.QueryTrackingBehavior;
					Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
					
					string raw;
					using (StreamReader sourceReader = File.OpenText(fileName))
					{
						raw = await sourceReader.ReadToEndAsync();
					}
					dynamic data = JObject.Parse(raw);

					await ProcessBrands(data.Brands);
					await ProcessVendors(data.Vendors);
					await ProcessUnits(data.Units);
					await ProcessCategories(data.Categories);
					await ProcessProducts(data.Products);

					await Context.SaveChangesAsync();

					Context.ChangeTracker.QueryTrackingBehavior = initialState ;
				}

				//				GetCatalogItemPictures(contentRootPath, picturePath);
			});
		}

		private AsyncRetryPolicy CreatePolicy(string prefix, int retries = 3)
		{
			return Policy.Handle<SqlException>().
				WaitAndRetryAsync(
					retryCount: retries,
					sleepDurationProvider: retry => System.TimeSpan.FromSeconds(5),
					onRetry: (exception, timeSpan, retry, ctx) =>
					{
						Logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
					}
				);
		}

		async Task ProcessBrands(dynamic items)
		{
			List<Brand> data = await Context.Brands.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					Context.Brands.Add(new Brand() { Name = item });
				}
			}
			await Context.SaveChangesAsync();
		}

		async Task ProcessVendors(dynamic items)
		{
			List<Vendor> data = await Context.Vendors.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					Context.Vendors.Add(new Vendor() { Name = item });
				}
			}
			await Context.SaveChangesAsync();
		}

		async Task ProcessUnits(dynamic items)
		{
			List<Unit> data = await Context.Units.ToListAsync();
			foreach (string item in items)
			{
				if (!data.Any(b => b.Name == item))
				{
					Context.Units.Add(new Unit() { Name = item });
				}
			}
			await Context.SaveChangesAsync();
		}

		async Task ProcessCategories(dynamic items)
		{
			List<Category> data = await Context.Categories.ToListAsync();
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
					Context.Categories.Add(category);
				}
			}
			int changes = await Context.SaveChangesAsync();
		}


		async Task ProcessProducts(dynamic items)
		{
			Logger.LogInformation("Enterring ProcessProducts");

			List<Brand> brands = await Context.Brands.ToListAsync();
			List<Vendor> vendors = await Context.Vendors.ToListAsync();
			List<Category> categories = await Context.Categories.ToListAsync();
			List<Unit> units = await Context.Units.ToListAsync();

			List<ItemCategory> pcs = new List<ItemCategory>();

			List<Item> data = await Context.Items.ToListAsync();
			foreach (dynamic item in items)
			{
				if (!data.Any(b => b.Name == (string)item.Name)) // && b.Count == (int)item.Count))
				{
					Brand brand = brands.FirstOrDefault(b => b.Name == (string)item.Brand);

					if (brand == null)
					{
						string name = item.Name;
						Logger.LogError("invalid item", name);
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
						Variant v = new Variant
						{
							Item = p
						};
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


					await Context.Items.AddAsync(p);
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
			await Context.SaveChangesAsync();
		}

	}
}
