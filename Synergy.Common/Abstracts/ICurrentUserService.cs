using System;
using System.Security.Principal;

namespace Synergy.Common.Abstracts
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }

        IPrincipal Principal { get; }

        IIdentity Identity { get; }

        bool IsInAllRoles(params string[] roles);

        bool IsInAnyRoles(params string[] roles);
    }
}
