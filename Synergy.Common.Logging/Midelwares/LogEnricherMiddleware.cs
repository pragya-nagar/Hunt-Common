using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Context;

namespace Synergy.Common.Logging.Midelwares
{
    public class LogEnricherMiddleware
    {
        private readonly RequestDelegate _next;

        public LogEnricherMiddleware(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext,
                                 ICorrelationContextAccessor correlationContextAccessor,
                                 IHostingEnvironment environment)
        {
            var claimsIdentity = httpContext.User?.Identity as ClaimsIdentity;
            LogContext.PushProperty("UserId", claimsIdentity?.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "unknown");
            LogContext.PushProperty("User", httpContext.User?.Identity?.Name ?? "unknown");
            LogContext.PushProperty("RequestIpAddress", httpContext.Connection.RemoteIpAddress.ToString());
            LogContext.PushProperty("CorrelationIdGUID", correlationContextAccessor.CorrelationContext?.CorrelationId);
            LogContext.PushProperty("RequestPath", httpContext.Request.GetDisplayUrl());
            LogContext.PushProperty("QueryString", httpContext.Request.QueryString.ToUriComponent());
            LogContext.PushProperty("RequestMethod", httpContext.Request.Method);
            LogContext.PushProperty("MachineName", Environment.MachineName);
            LogContext.PushProperty("Environment", $"{environment.ApplicationName}-{environment.EnvironmentName}");

            await _next(httpContext).ConfigureAwait(false);
        }
    }
}
