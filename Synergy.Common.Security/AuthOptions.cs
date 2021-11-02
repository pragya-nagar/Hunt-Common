using System.Collections.Generic;

namespace Synergy.Common.Security
{
    public class AuthOptions
    {
        public string Issuer { get; set; } = "CazCreek";

        public string Audience { get; set; } = "Synergy";

        public string SecretKey { get; set; }

        public int JwtSessionTime { get; set; } = 300;

        public int RefreshTokenSessionTime { get; set; } = 900;

        public IDictionary<string, bool> Modules { get; set; }

        public bool UseSignalRAuth { get; set; } = false;
    }
}