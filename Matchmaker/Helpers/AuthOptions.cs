using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Matchmaker.Helpers
{
    public class AuthOptions
    {
		public const string ISSUER = "Matchmaker";  
        public const string AUDIENCE = "Player";
        const string KEY = "super_sekret_security_key!";
        public const int LIFETIME = 60;

        public static SymmetricSecurityKey GetSymmetricSecurityKey() 
            => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}
