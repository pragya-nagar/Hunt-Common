using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Synergy.Common.Security.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CheckPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private const string ClaimType = "Permissions";

        private readonly string[] _permissions;

        public CheckPermissionAttribute(params string[] permissions)
        {
            if (permissions.Any() != true || permissions.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Permissions list can not be empty.", nameof(permissions));
            }

            this._permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == false)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionClaimValue = context.HttpContext.User.Claims.Where(c => c.Type == ClaimType).Select(x => x.Value).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(permissionClaimValue))
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissions = JsonConvert.DeserializeObject<List<string>>(permissionClaimValue);

            var hasClaim = this._permissions.Any(required =>
            {
                var expressionString = required
                    .Replace(".", "[.]", StringComparison.InvariantCulture)
                    .Replace("**", @"[\w\.]+", StringComparison.InvariantCulture)
                    .Replace("*", @"\w+", StringComparison.InvariantCulture);

                var reg = new Regex(
                    $"^{expressionString}$",
                    RegexOptions.IgnorePatternWhitespace
                    | RegexOptions.Singleline
                    | RegexOptions.IgnorePatternWhitespace
                    | RegexOptions.CultureInvariant);

                return permissions.Any(permission => reg.IsMatch(permission));
            });

            if (hasClaim == false)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
