using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MasterApi.Core.Filters;
using MasterApi.Core.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MasterApi.Core.Services;

namespace MasterApi.Web.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private static IService<ExceptionLog> _exceptionLogService;
        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        public static IDictionary<Type, HttpStatusCode> Mappings
        {
            get;
            //Set is private to make it singleton
            private set;
        }

        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="exceptionLogService">The exception log service.</param>
        public ErrorHandlingMiddleware(RequestDelegate next, IService<ExceptionLog> exceptionLogService)
        {
            _next = next;
            _exceptionLogService = exceptionLogService;

            Mappings = new Dictionary<Type, HttpStatusCode>
            {
                {typeof (ArgumentNullException), HttpStatusCode.BadRequest},
                {typeof (ArgumentException), HttpStatusCode.BadRequest},
                {typeof (IndexOutOfRangeException), HttpStatusCode.BadRequest},
                {typeof (DivideByZeroException), HttpStatusCode.BadRequest},
                {typeof (InvalidOperationException), HttpStatusCode.BadRequest},
                {typeof (ValidationException), HttpStatusCode.BadRequest},
                {typeof (CustomValidationException), HttpStatusCode.BadRequest},
                {typeof (DbUpdateException), HttpStatusCode.InternalServerError},
                {typeof (ForbiddenException), HttpStatusCode.Forbidden},
                {typeof (NotFoundException), HttpStatusCode.NotFound},
                {typeof (SecurityTokenExpiredException), HttpStatusCode.Unauthorized},
                {typeof (NotImplementedException), HttpStatusCode.BadRequest}
            };
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context == null || exception == null) return;

            var type = exception.GetType();
            var httpStatusCode = HttpStatusCode.InternalServerError;
            var externalMessage = exception.Message;
            var internalMessage = externalMessage;

            if (Mappings.ContainsKey(type))
            {
                httpStatusCode = Mappings[exception.GetType()];
                if (type == typeof(DbUpdateException))
                {
                    if (exception.InnerException != null)
                    {
                        internalMessage = exception.InnerException.InnerException != null ?
                                           exception.InnerException.InnerException.Message :
                                           exception.InnerException.Message;

                        internalMessage = string.Format("{0} - {1}", internalMessage, externalMessage);
                        externalMessage = "Exception thrown by System.Data.Entity.DbContext when the saving of changes to the database fails.";
                    }
                }
            }
            else
            {
                if (exception is NotImplementedException)
                {
                    externalMessage = exception.Message;
                    httpStatusCode = HttpStatusCode.NotImplemented;
                }
            }

            if (!(exception is CustomValidationException) &&
                !(exception is ForbiddenException) &&
                !(exception is NotFoundException)
               )
            {
                await LogError(context, exception, internalMessage);
            }

            await WriteExceptionAsync(context, exception, httpStatusCode, externalMessage).ConfigureAwait(false);
        }

        private static async Task WriteExceptionAsync(HttpContext context, Exception exception, HttpStatusCode code, string message)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)code;
            await response.WriteAsync(JsonConvert.SerializeObject(new
            {
                error = new
                {
                    message = message ?? exception.Message,
                    exception = exception.GetType().Name
                }
            })).ConfigureAwait(false);
        }

        private static async Task LogError(HttpContext context, Exception exception, string message)
        {
            int? userId = null;
            if (context.User.Identity.IsAuthenticated)
            {
                userId = int.Parse(context.User.Claims.FirstOrDefault(x=>x.Type == ClaimTypes.NameIdentifier).Value);
            }

            var err = new ExceptionLog
            {
                UserId = userId,
                Message = message,
                Source = exception.Source,
                StackTrace = exception.StackTrace,
                HResult = exception.HResult,
                RequestUri = context.Request.Path,
                Method = context.Request.Method
            };

            await _exceptionLogService.Repository.InsertAsync(err, true);
        }
    }
}

// a real application.
//app.UseExceptionHandler(appBuilder =>
//{
//    appBuilder.Use(async (context, next) =>
//    {
//        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
//        context.Response.Headers.Add("Content-Type", "*");

//        var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
//        // This should be much more intelligent - at the moment only expired 
//        // security tokens are caught - might be worth checking other possible 
//        // exceptions such as an invalid signature.
//        if (error?.Error is SecurityTokenExpiredException)
//        {
//            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
//            // What you choose to return here is up to you, in this case a simple 
//            // bit of JSON to say you're no longer authenticated.
//            context.Response.ContentType = "application/json";
//            await context.Response.WriteAsync(
//                JsonConvert.SerializeObject(
//                    new { authenticated = false, tokenExpired = true }));
//        }
//        else if (error?.Error != null)
//        {
//            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
//            context.Response.ContentType = "application/json";
//            // TODO: Shouldn't pass the exception message straight out, change this.
//            await context.Response.WriteAsync(
//                JsonConvert.SerializeObject
//                (new { success = false, error = error.Error.Message }));
//        }
//        // We're not trying to handle anything else so just let the default 
//        // handler handle.
//        else await next();
//    });
//});