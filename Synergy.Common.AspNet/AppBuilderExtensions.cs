using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Synergy.Common.Abstracts;
using Synergy.Common.Extensions;

namespace Synergy.Common.AspNet
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseHealthChecks("/api/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            return app.UseMiddleware<OperationContextMiddleware>();
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, string swaggerEndpointName)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((document, request) =>
                {
                    var paths = document.Paths.ToDictionary(item => Regex.Replace(item.Key,
                            "((?<route>[A-Za-z]+)|([{].+?[}]))+",
                            e => e.Groups["route"].Success
                                ? e.Value.ToLowerInvariant()
                                : e.Value),
                        item => item.Value);

                    document.Paths.Clear();
                    foreach (var (key, value) in paths)
                    {
                        document.Paths.Add(key, value);
                    }
                });
            }).UseSwaggerUI(options =>
            {
                options.RoutePrefix = "swagger";
                options.SwaggerEndpoint("/swagger/v1/swagger.json", swaggerEndpointName);
            });

            return app.UseMiddleware<OperationContextMiddleware>();
        }

        public static IApplicationBuilder UseOperationContext(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<OperationContextMiddleware>();
        }

        public static IApplicationBuilder UseVersion(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Map("/api/version", builder =>
            {
                builder.Run(async context =>
                {
                    var runtimeContext = context.RequestServices.GetRequiredService<IRunTimeContext>();
                    context.Response.ContentType = "application/json";
                    var responseBody = JsonConvert.SerializeObject(new
                    {
                        Version = runtimeContext.Version,
                        Uptime = runtimeContext.Uptime.ToString(@"dd\.hh\:mm\:ss", CultureInfo.InvariantCulture),
                    });

                    await context.Response.WriteAsync(responseBody);
                });
            });

            return app;
        }

        public static void UseStartupLogging(this IApplicationBuilder app)
        {
            var factory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = factory.CreateLogger(Assembly.GetEntryAssembly().GetName().Name);

            var runtimeContext = app.ApplicationServices.GetRequiredService<IRunTimeContext>();

            logger.LogInformation("Starting service, version={version}.", runtimeContext.Version);
        }
    }
}