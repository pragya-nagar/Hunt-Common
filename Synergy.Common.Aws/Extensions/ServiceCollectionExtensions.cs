using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Hosting;
using Amazon;

namespace Synergy.Common.Aws.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IWebHostBuilder UseDefaultSettings<T>(this IWebHostBuilder builder) where T : class
        {
            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            builder
                .UseEnvironment(environmentName)
                .ConfigureAppConfiguration((a, b) => a.ConfigureAppConfiguration(b))
                .UseStartup<T>();
            return builder;
        }

        public static void ConfigureAppConfiguration(this WebHostBuilderContext hostingContext, IConfigurationBuilder builder)
        {
            if (hostingContext.HostingEnvironment.IsDevelopment() == false)
            {
                builder.AddSystemsManager(configureSource =>
                {
                    var env = hostingContext.HostingEnvironment;

                    // Parameter Store prefix to pull configuration data from.
                    configureSource.Path = $"/hunt-caz-creek-synergy/{env.EnvironmentName}";

                    // Reload configuration data every 15 minutes.
                    configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
                });
            }
        }

        public static RegionEndpoint GetRegionEndPoint(this IConfiguration configuration)
        {
            var region = RegionEndpoint.USEast1;
            region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);

            if (Equals(region.DisplayName, "Unknown"))
            {
                region = RegionEndpoint.USEast1;
            }

            return region;
        }
    }
}
