using Microsoft.AspNetCore.Hosting;
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

namespace Catalog.API.Infrastructure
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
					string raw = await File.ReadAllTextAsync(fileName);
					dynamic data = JObject.Parse(raw);

					//					await processBrands(data.Brands, context, logger);
					//					await processVendors(data.Vendors, context, logger);
					//					await processUnits(data.Units, context, logger);
					//					await processCategories(data.Categories, context, logger);
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

	}
}
