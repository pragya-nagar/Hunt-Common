using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Synergy.Common.Security
{
    public class DevelopAuthOptions : AuthenticationSchemeOptions
    {
        public IEnumerable<Claim> Claims { get; set; }
    }
}
