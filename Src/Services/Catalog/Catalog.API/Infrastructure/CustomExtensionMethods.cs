﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using ROL.Services.Catalog.DAL;
using ROL.Services.Common.API.Infrastructure.Filters;
using Swashbuckle.AspNetCore.Swagger;
//using System;
using System.Collections.Generic;
//using System.Data.Common;
//using System.Reflection;
//using HealthChecks.AzureStorage;
//using HealthChecks.SqlServer;
//using Microsoft.AspNetCore.HealthChecks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace ROL.Services.Catalog.API.Infrastructure
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

        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var accountName = configuration.GetValue<string>("AzureStorageAccountName");
            var accountKey = configuration.GetValue<string>("AzureStorageAccountKey");

            var hcBuilder = services.AddHealthChecks();

            hcBuilder
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(
                    configuration["ConnectionString"],
                    name: "CatalogDB-check",
                    tags: new string[] { "catalogdb" });

            if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(accountKey))
            {
                hcBuilder
                    .AddAzureBlobStorage(
                        $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net",
                        name: "catalog-storage-check",
                        tags: new string[] { "catalogstorage" });
            }

            //if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            //{
            //	hcBuilder
            //		.AddAzureServiceBusTopic(
            //			configuration["EventBusConnection"],
            //			topicName: "eshop_event_bus",
            //			name: "catalog-servicebus-check",
            //			tags: new string[] { "servicebus" });
            //}
            //else
            //{
            //	hcBuilder
            //		.AddRabbitMQ(
            //			$"amqp://{configuration["EventBusConnection"]}",
            //			name: "catalog-rabbitmqbus-check",
            //			tags: new string[] { "rabbitmqbus" });
            //}

            return services;
        }

        public static IServiceCollection AddCustomMVC(this IServiceCollection services, IConfiguration configuration)
        {
            IHealthChecksBuilder hcBuilder = services.AddHealthChecks();

            hcBuilder
                .AddSqlServer(configuration["ConnectionString"]);



            services.AddHealthChecks(checks =>
            {
                //int minutes = 1;
                //if (int.TryParse(configuration["HealthCheck:Timeout"], out int minutesParsed))
                //{
                //	minutes = minutesParsed;
                //}
                //checks.(configuration["ConnectionString"], name: "CatalogDb", , TimeSpan.FromMinutes(minutes));

                //string accountName = configuration.GetValue<string>("AzureStorageAccountName");
                //string accountKey = configuration.GetValue<string>("AzureStorageAccountKey");
                //if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(accountKey))
                //{
                //	checks.AddAzureBlobStorage(accountName, accountKey);
                //}
            });

            services.AddMvc(options =>
            {
                //                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            })
            .AddControllersAsServices()
            .AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                }
            )
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

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
                options.ConfigureFromSettings<Context>(configuration);
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
                //                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "eShopOnContainers - Catalog HTTP API",
                    Version = "v1",
                    Description = "The Catalog Microservice HTTP API. This is a Data-Driven/CRUD microservice sample",
                    //					TermsOfService = "Terms Of Service"
                });
                //options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                //{
                //    Type = SecuritySchemeType.OAuth2,
                //    Flows = new OpenApiOAuthFlows
                //    {
                //        Implicit = new OpenApiOAuthFlow
                //        {
                //            AuthorizationUrl = new System.Uri( $"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                //            Scopes = new Dictionary<string, string>()
                //            {
                //                { "catalog", "Catalog API" }
                //            },
                //        TokenUrl = new System.Uri( $"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                //        }
                //    }

                //});

                //                options.OperationFilter<AuthorizeCheckOperationFilter>();

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
