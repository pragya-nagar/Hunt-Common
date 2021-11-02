using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Synergy.Common.Security
{
    public class DevelopAuthenticationHandler : AuthenticationHandler<DevelopAuthOptions>
    {
        public DevelopAuthenticationHandler(IOptionsMonitor<DevelopAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (this.Options.Claims.Any() == false)
            {
                throw new InvalidOperationException("Claims must be provided in options");
            }

            var identity = new ClaimsIdentity(this.Options.Claims, this.Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            return await Task.FromResult(AuthenticateResult.Success(ticket)).ConfigureAwait(false);
        }
    }
}
