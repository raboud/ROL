using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Autofac;
//using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using ROL.Services.Catalog.API.Infrastructure;

namespace ROL.Services.Catalog.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services
				.AddAppInsight(Configuration)
				.AddCustomMVC(Configuration)
				.AddCustomDbContext(Configuration)
				.AddCustomOptions(Configuration)
				.AddIntegrationServices(Configuration)
				.AddEventBus(Configuration)
				.AddSwagger(Configuration);

			services.AddAutoMapper(x => x.AddProfile(new AutoMapperProfile()));

			return services.BuildServiceProvider();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			string pathBase = Configuration["PATH_BASE"];

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

//			app.UseHttpsRedirection();
			app.UseMvc();
			app.UseSwagger()
			  .UseSwaggerUI(c =>
			  {
				  c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Catalog.API V1");
			  });
		}
	}
}
