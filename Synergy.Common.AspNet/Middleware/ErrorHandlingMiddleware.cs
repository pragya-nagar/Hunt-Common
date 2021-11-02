using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Synergy.Common.Exceptions;

namespace Synergy.Common.AspNet.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, RequestDelegate next)
        {
            this._logger = logger;
            this._next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this._next(context).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var logMessage = exception.ToString().Replace("{", "{{").Replace("}", "}}");
            var userMessage = exception.Message;

            switch (exception)
            {
                case NotFoundException _:
                    code = HttpStatusCode.NotFound;
                    this._logger.LogWarning("Entity not found. {StatusCode} {ExceptionMessage}", code, logMessage);
                    break;
                case ModelStateException _:
                    this._logger.LogWarning("Bad Request. {StatusCode} {ExceptionMessage}", code, logMessage);
                    code = HttpStatusCode.BadRequest;
                    break;
                case NotAcceptableException _:
                    this._logger.LogWarning("Bad Request. {StatusCode} {ExceptionMessage}", code, logMessage);
                    code = HttpStatusCode.BadRequest;
                    break;
                default:
                    this._logger.LogError(exception, logMessage);
                    userMessage = "Internal server error";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            await context.Response.WriteAsync(userMessage).ConfigureAwait(false);
        }
    }
}
