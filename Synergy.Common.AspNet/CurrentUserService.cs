using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http;
using Synergy.Common.Abstracts;

namespace Synergy.Common.AspNet
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _context;

        public CurrentUserService(IHttpContextAccessor context)
        {
            this._context = context;
        }

        public Guid UserId => Guid.TryParse(((ClaimsPrincipal)this.Principal)?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;

        public IPrincipal Principal => ClaimsPrincipal.Current ?? this._context.HttpContext?.User;

        public IIdentity Identity => this.Principal.Identity;

        private IEnumerable<string> Roles => ((ClaimsPrincipal)this.Principal).Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value);

        public bool IsInAllRoles(params string[] roles)
        {
            return roles.Except(this.Roles).Any() == false;
        }

        public bool IsInAnyRoles(params string[] roles)
        {
            return roles.Intersect(this.Roles).Any();
        }
    }
}
