using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Synergy.Common.Abstracts;

namespace Synergy.Common.AspNet
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCors(this IServiceCollection services, string policyName = "synergy")
        {
            services.AddCors(x => x.AddPolicy(policyName, builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(60))
                    .WithExposedHeaders("Content-Disposition")
                    .WithExposedHeaders("X-Correlation-Id")));

            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, string npgsqlConnectionString, string name = "Database")
        {
            services.AddHealthChecks().AddNpgSql(npgsqlConnectionString, name: name);

            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services, string appModuleName)
        {
            services.AddSwaggerGen(opt =>
            {
                opt.DescribeAllEnumsAsStrings();
                opt.SwaggerDoc("v1",
                    new Info
                    {
                        Version = "v1",
                        Title = appModuleName,
                    });
                opt.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    Type = "apiKey",
                    In = "header",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                });
                opt.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Array.Empty<string>() },
                });
                opt.SwaggerGeneratorOptions.OperationFilters.Add(new OperationIdFilter());
            });

            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services, Assembly[] assemblies)
        {
            services.AddAutoMapper(mapperConfig =>
            {
                mapperConfig.ForAllPropertyMaps(map => map.SourceType?.IsEnum == true, (map, exp) =>
                {
                    exp.PreCondition((source, target, context) =>
                    {
                        var info = (PropertyInfo)map.SourceMember;
                        return Enum.IsDefined(map.SourceType, info.GetValue(source));
                    });
                });
            }, assemblies);

            return services;
        }

        public static IServiceCollection AddOperationContext(this IServiceCollection services)
        {
            services.TryAddSingleton<OperationContextStore>();
            services.TryAddSingleton<IOperationContextFactory>(s => s.GetRequiredService<OperationContextStore>());
            services.TryAddSingleton<IOperationContextAccessor>(s => s.GetRequiredService<OperationContextStore>());

            return services;
        }

        public static IServiceCollection AddRunTimeContext(this IServiceCollection services)
        {
            services.TryAddSingleton<IRunTimeContext, RunTimeContext>();

            return services;
        }

        public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
        {
            services.TryAddTransient<ICurrentUserService, CurrentUserService>();

            return services;
        }

        public static IServiceCollection AddClock(this IServiceCollection services)
        {
            services.TryAddTransient<IClockService, ClockService>();

            return services;
        }

        public static IServiceCollection AddDefaultApiContext(this IServiceCollection services)
        {
            return services
                .AddClock()
                .AddOperationContext()
                .AddCurrentUserService()
                .AddRunTimeContext();
        }

        private class OperationIdFilter : IOperationFilter
        {
            public void Apply(Operation operation, OperationFilterContext context)
            {
                operation.OperationId = context.MethodInfo.Name + context.MethodInfo.GetParameters()
                    .Aggregate("_", (acc, cur) => cur.ParameterType == typeof(CancellationToken)
                                                ? acc
                                                : acc + "_" + cur.ParameterType.Name);
            }
        }
    }
}