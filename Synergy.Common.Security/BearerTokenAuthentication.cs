using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Synergy.Common.Security.Extensions;

namespace Synergy.Common.Security
{
    public static class BearerTokenAuthentication
    {
        public static void AddBearerTokenAuthentication(this IServiceCollection services,
                                                        Action<AuthOptions> configureOptions)
        {
            var authOptions = new AuthOptions();
            configureOptions?.Invoke(authOptions);

            if (string.IsNullOrEmpty(authOptions.SecretKey))
            {
                throw new InvalidOperationException("Bearer authorization can not be configured without secret key");
            }

            var tokenParams = new TokenValidationParameters
                              {
                                  ValidateIssuer = true,
                                  ValidateAudience = true,
                                  ValidateLifetime = true,
                                  ValidateIssuerSigningKey = true,
                                  ValidIssuer = authOptions.Issuer,
                                  ValidAudience = authOptions.Audience,
                                  IssuerSigningKey = authOptions.GetSecurityKey(),
                              };

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = tokenParams;

                        if (authOptions.UseSignalRAuth)
                        {
                            options.Events = new JwtBearerEvents
                            {
                                OnMessageReceived = context =>
                                {
                                    var accessToken = context.Request.Query["access_token"];
                                    var path = context.HttpContext.Request.Path;
                                    if (string.IsNullOrEmpty(accessToken) == false && path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                                    {
                                        context.Token = accessToken;
                                    }

                                    return Task.CompletedTask;
                                },
                            };
                        }
                    });
        }
    }
}