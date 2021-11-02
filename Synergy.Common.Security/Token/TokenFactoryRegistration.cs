using System;

using Microsoft.Extensions.DependencyInjection;

namespace Synergy.Common.Security.Token
{
    public static class TokenFactoryRegistration
    {
        public static IServiceCollection RegisterTokenFactory(this IServiceCollection services, Action<AuthOptions> configureOptions)
        {
            var authOptions = new AuthOptions();
            configureOptions?.Invoke(authOptions);

            services.AddSingleton<ITokenFactory>(sp => new TokenFactory(authOptions));

            return services;
        }
    }
}