using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using ROL.Services.Common.API.Infrastructure.Filters;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Catalog.API.Infrastructure
{
	public static class CustomExtensionMethods
	{
		public static IServiceCollection AddAppInsight(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddApplicationInsightsTelemetry(configuration);
			//string orchestratorType = configuration.GetValue<string>("OrchestratorType");

			//if (orchestratorType?.ToUpper() == "K8S")
			//{
			//	// Enable K8s telemetry initializer
			//	services.EnableKubernetes();
			//}
			//if (orchestratorType?.ToUpper() == "SF")
			//{
			//	// Enable SF telemetry initializer
			//	services.AddSingleton<ITelemetryInitializer>((serviceProvider) =>
			//		new FabricTelemetryInitializer());
			//}

			return services;
		}

		public static IServiceCollection AddCustomMVC(this IServiceCollection services, IConfiguration configuration)
		{
			//services.AddHealthChecks(checks =>
			//{
			//	int minutes = 1;
			//	if (int.TryParse(configuration["HealthCheck:Timeout"], out int minutesParsed))
			//	{
			//		minutes = minutesParsed;
			//	}
			//	checks.AddSqlCheck("CatalogDb", configuration["ConnectionString"], TimeSpan.FromMinutes(minutes));

			//	string accountName = configuration.GetValue<string>("AzureStorageAccountName");
			//	string accountKey = configuration.GetValue<string>("AzureStorageAccountKey");
			//	if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(accountKey))
			//	{
			//		checks.AddAzureBlobStorageCheck(accountName, accountKey);
			//	}
			//});

			services.AddMvc(options =>
			{
				options.Filters.Add(typeof(HttpGlobalExceptionFilter));
			})
			.AddControllersAsServices()
			.AddJsonOptions(
				options => {
					options.SerializerSettings.ContractResolver = new DefaultContractResolver();
					options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				}
			)
			.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddMvcCore()
				.AddAuthorization();

			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
					builder => builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials());
			});

			return services;
		}

		public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<Context>(options =>
			{
				options.UseSqlServer(configuration["ConnectionString"],
									 sqlServerOptionsAction: sqlOptions =>
									 {
										 sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
										 //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
										 sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
									 });

				// Changing default behavior when client evaluation occurs to throw. 
				// Default in EF Core would be to log a warning when client evaluation is performed.
				options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
				//Check Client vs. Server evaluation: https://docs.microsoft.com/en-us/ef/core/querying/client-eval
			});

			//services.AddDbContext<IntegrationEventLogContext>(options =>
			//{
			//	options.UseSqlServer(configuration["ConnectionString"],
			//						 sqlServerOptionsAction: sqlOptions =>
			//						 {
			//							 sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
			//							 //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
			//							 sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
			//						 });
			//});

			return services;
		}

		public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<Settings>(configuration);
			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					ValidationProblemDetails problemDetails = new ValidationProblemDetails(context.ModelState)
					{
						Instance = context.HttpContext.Request.Path,
						Status = StatusCodes.Status400BadRequest,
						Detail = "Please refer to the errors property for additional details."
					};

					return new BadRequestObjectResult(problemDetails)
					{
						ContentTypes = { "application/problem+json", "application/problem+xml" }
					};
				};
			});

			return services;
		}

		public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSwaggerGen(options =>
			{
				options.DescribeAllEnumsAsStrings();
				options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
				{
					Title = "eShopOnContainers - Catalog HTTP API",
					Version = "v1",
					Description = "The Catalog Microservice HTTP API. This is a Data-Driven/CRUD microservice sample",
					TermsOfService = "Terms Of Service"
				});
				options.AddSecurityDefinition("oauth2", new OAuth2Scheme
				{
					Type = "oauth2",
					Flow = "implicit",
					AuthorizationUrl = $"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize",
					TokenUrl = $"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token",
					Scopes = new Dictionary<string, string>()
					{
						{ "catalog", "Catalog API" }
					}
				});

				options.OperationFilter<AuthorizeCheckOperationFilter>();

			});

			return services;

		}

		public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
		{
		//	services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
		//	   sp => (DbConnection c) => new IntegrationEventLogService(c));

		//	services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

		//	if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
		//	{
		//		services.AddSingleton<IServiceBusPersisterConnection>(sp =>
		//		{
		//			var settings = sp.GetRequiredService<IOptions<CatalogSettings>>().Value;
		//			var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();

		//			var serviceBusConnection = new ServiceBusConnectionStringBuilder(settings.EventBusConnection);

		//			return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
		//		});
		//	}
		//	else
		//	{
		//		services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
		//		{
		//			var settings = sp.GetRequiredService<IOptions<CatalogSettings>>().Value;
		//			var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

		//			var factory = new ConnectionFactory()
		//			{
		//				HostName = configuration["EventBusConnection"]
		//			};

		//			if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
		//			{
		//				factory.UserName = configuration["EventBusUserName"];
		//			}

		//			if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
		//			{
		//				factory.Password = configuration["EventBusPassword"];
		//			}

		//			int retryCount = 5;
		//			if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
		//			{
		//				retryCount = int.Parse(configuration["EventBusRetryCount"]);
		//			}

		//			return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
		//		});
		//	}

			return services;
		}

		public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
		{
		//	string subscriptionClientName = configuration["SubscriptionClientName"];

		//	if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
		//	{
		//		services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
		//		{
		//			var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
		//			var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
		//			var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
		//			var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

		//			return new EventBusServiceBus(serviceBusPersisterConnection, logger,
		//				eventBusSubcriptionsManager, subscriptionClientName, iLifetimeScope);
		//		});

		//	}
		//	else
		//	{
		//		services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
		//		{
		//			var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
		//			var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
		//			var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
		//			var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

		//			int retryCount = 5;
		//			if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
		//			{
		//				retryCount = int.Parse(configuration["EventBusRetryCount"]);
		//			}

		//			return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
		//		});
		//	}

		//	services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
		//	services.AddTransient<OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
		//	services.AddTransient<OrderStatusChangedToPaidIntegrationEventHandler>();

			return services;
		}
	}
}
