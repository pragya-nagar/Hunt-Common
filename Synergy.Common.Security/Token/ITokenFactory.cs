using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Synergy.Common.Security.UserRoleModels;

namespace Synergy.Common.Security.Token
{
    public interface ITokenFactory
    {
        AuthOptions Options { get; }

        JwtSecurityToken GetJwt(Guid userId, string name, string email, List<string> roles, List<string> permissions, List<UserDepartmentRoleModel> userDepartmentRoles);

        string GetJwtString(Guid userId, string name, string email, List<string> roles, List<string> permissions, List<UserDepartmentRoleModel> userDepartmentRoles);

        string GetRefreshToken();
    }
}