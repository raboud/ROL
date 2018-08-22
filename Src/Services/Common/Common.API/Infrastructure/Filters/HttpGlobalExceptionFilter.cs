using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using ROL.Services.Common.API.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ROL.Services.Common.API.Infrastructure.Filters
{
	public class HttpGlobalExceptionFilter : HttpGlobalExceptionFilter<DomainException>
	{
		public HttpGlobalExceptionFilter(IHostingEnvironment env, ILogger<HttpGlobalExceptionFilter<DomainException>> logger) : base(env, logger)
		{
		}
	}


	public class HttpGlobalExceptionFilter<T> : IExceptionFilter
	{
		protected readonly IHostingEnvironment env;
		protected readonly ILogger<HttpGlobalExceptionFilter<T>> logger;

		public HttpGlobalExceptionFilter(IHostingEnvironment env, ILogger<HttpGlobalExceptionFilter<T>> logger)
		{
			this.env = env;
			this.logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			logger.LogError(new EventId(context.Exception.HResult),
				context.Exception,
				context.Exception.Message);

			if (context.Exception.GetType() == typeof(T))
			{
				ValidationProblemDetails problemDetails = new ValidationProblemDetails()
				{
					Instance = context.HttpContext.Request.Path,
					Status = StatusCodes.Status400BadRequest,
					Detail = "Please refer to the errors property for additional details."
				};

				problemDetails.Errors.Add("DomainValidations", new string[] { context.Exception.Message.ToString() });

				context.Result = new BadRequestObjectResult(problemDetails);
				context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			}
			else
			{
				JsonErrorResponse json = new JsonErrorResponse
				{
					Messages = new[] { "An error ocurred." }
				};

				if (env.IsDevelopment())
				{
					json.DeveloperMeesage = context.Exception;
				}

				context.Result = new InternalServerErrorObjectResult(json);
				context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
			context.ExceptionHandled = true;
		}

		private class JsonErrorResponse
		{
			public string[] Messages { get; set; }

			public object DeveloperMeesage { get; set; }
		}
	}
}
