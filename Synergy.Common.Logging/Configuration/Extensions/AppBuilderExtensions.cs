using CorrelationId;
using Microsoft.AspNetCore.Builder;
using Synergy.Common.Logging.Midelwares;

namespace Synergy.Common.Logging.Configuration
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseCorrelationLogging(this IApplicationBuilder app)
        {
            app.UseCorrelationId(new CorrelationIdOptions
            {
                Header = "X-Correlation-ID",
                UseGuidForCorrelationId = true,
                UpdateTraceIdentifier = false,
            });

            app.UseMiddleware<LogEnricherMiddleware>();

            return app;
        }
    }
}
