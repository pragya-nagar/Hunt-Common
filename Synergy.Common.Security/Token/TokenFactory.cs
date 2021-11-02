using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Synergy.Common.Security.Extensions;
using Synergy.Common.Security.UserRoleModels;

namespace Synergy.Common.Security.Token
{
    public class TokenFactory : ITokenFactory
    {
        public TokenFactory(AuthOptions authOptions)
        {
            Options = authOptions;
        }

        public AuthOptions Options { get; }

        public JwtSecurityToken GetJwt(Guid userId, string name, string email, List<string> roles, List<string> permissions, List<UserDepartmentRoleModel> userDepartmentRoles)
        {
            var claims = GetClaims(userId, name, email, roles, permissions, userDepartmentRoles);

            var credentials = new SigningCredentials(Options.GetSecurityKey(), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                                             issuer: Options.Issuer,
                                             audience: Options.Audience,
                                             claims: claims,
                                             expires: DateTime.Now.AddSeconds(Options.JwtSessionTime),
                                             signingCredentials: credentials);
            return token;
        }

        public string GetJwtString(Guid userId, string name, string email, List<string> roles, List<string> permissions, List<UserDepartmentRoleModel> userDepartmentRoles)
        {
            var token = GetJwt(userId, name, email, roles, permissions, userDepartmentRoles);
            var result = new JwtSecurityTokenHandler().WriteToken(token);
            return result;
        }

        public string GetRefreshToken()
        {
            var guid = Guid.NewGuid();
            string result = Convert.ToBase64String(guid.ToByteArray())
                                   .Replace("=", string.Empty, StringComparison.InvariantCulture)
                                   .Replace("+", string.Empty, StringComparison.InvariantCulture)
                                   .Replace("/", string.Empty, StringComparison.InvariantCulture)
                                   .Replace("\\", string.Empty, StringComparison.InvariantCulture);
            return result;
        }

        private Claim[] GetClaims(Guid userId, string name, string email, List<string> roles, List<string> permissions, List<UserDepartmentRoleModel> userDepartmentRoles)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, JsonConvert.SerializeObject(roles)),
                new Claim("Permissions", JsonConvert.SerializeObject(permissions)),
            };

            IEnumerable<Claim> moduleClaims = Options?.Modules?.Where(m => m.Value == true).Select(m =>
            {
                return new Claim("AppModule", m.Key);
            });

            if (moduleClaims != null)
            {
                claims.AddRange(moduleClaims);
            }

            claims.Add(new Claim("Departments", JsonConvert.SerializeObject(userDepartmentRoles)));

            return claims.ToArray();
        }
    }
}