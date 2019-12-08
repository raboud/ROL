using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ROL.Services.Catalog.DAL;
//using ROL.Services.Catalog.DTO;
using System;
using System.IO;
using System.Linq;

namespace Catalog.DTO.Tests.Unit
{
    // help
    public class DatabaseFixture : IDisposable
	{
		public ServiceProvider serviceProvider { get; set; }

		public Context Context { get; set; }
		public IConfiguration Config { get; set; }
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
}
