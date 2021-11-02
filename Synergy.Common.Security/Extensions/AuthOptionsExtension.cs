using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace Synergy.Common.Security.Extensions
{
    public static class AuthOptionsExtension
    {
        public static SymmetricSecurityKey GetSecurityKey(this AuthOptions authOptions)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey));
            return securityKey;
        }
    }
}