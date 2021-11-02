using System;
using System.Reflection;
using Amazon;
using Amazon.CloudWatchLogs;
using CorrelationId;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.Elasticsearch;
using Synergy.Common.Logging.CloudWatch;

namespace Synergy.Common.Logging.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSerilogLogging(this IServiceCollection services,
                                                           IConfiguration configuration)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.AddSerilog(dispose: true);
            });

            services.AddCorrelationId();

            var config = new SinksConfiguration();
            configuration.GetSection("Logging:Sinks").Bind(config);

            var logConfig = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", Assembly.GetEntryAssembly().GetName().Name)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                .AddExternalLogging(config)
                .SetMinimumLevel(config)
                .SetLevelOverrides(config);

            Log.Logger = logConfig.CreateLogger();

            return services;
        }

        private static LoggerConfiguration SetMinimumLevel(this LoggerConfiguration logConfig,  SinksConfiguration config)
        {
            return config.MinimumLevel.HasValue
                    ? logConfig.MinimumLevel.Is(config.MinimumLevel.Value)
                    : logConfig.MinimumLevel.Debug();
        }

        private static LoggerConfiguration SetLevelOverrides(this LoggerConfiguration logConfig, SinksConfiguration configuration)
        {
            if (configuration.LevelOverride == null)
            {
                return logConfig;
            }

            foreach (var logEventLevel in configuration.LevelOverride)
            {
                logConfig = logConfig.MinimumLevel.Override(logEventLevel.Key, logEventLevel.Value);
            }

            return logConfig;
        }

        private static LoggerConfiguration AddExternalLogging(this LoggerConfiguration logConfig, SinksConfiguration configuration)
        {
            if (configuration == null)
            {
                return logConfig;
            }

            if (configuration.CloudWatch != null)
            {
                logConfig = logConfig.WriteTo.AmazonCloudWatch(
                    new DefaultCloudWatchSinkOptions(configuration.CloudWatch.LogGroupName),
                    new AmazonCloudWatchLogsClient(RegionEndpoint.GetBySystemName(configuration.CloudWatch.Region)));
            }

            if (configuration.ElasticSearch != null)
            {
                if (string.IsNullOrWhiteSpace(configuration.ElasticSearch.AWSRegion))
                {
                    logConfig = logConfig.WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(configuration.ElasticSearch.Uri))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                        });
                }
                else
                {
                    logConfig = logConfig.WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(configuration.ElasticSearch.Uri))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,

                            ModifyConnectionSettings = conn =>
                            {
                                var httpConnection = new AwsHttpConnection(configuration.ElasticSearch.AWSRegion);
                                var pool = new SingleNodeConnectionPool(new Uri(configuration.ElasticSearch.Uri));
                                return new ConnectionConfiguration(pool, httpConnection);
                            },
                        });
                }
            }

            return logConfig;
        }
    }
}
