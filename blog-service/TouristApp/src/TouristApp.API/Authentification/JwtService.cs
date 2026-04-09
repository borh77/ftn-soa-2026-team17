using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


namespace TouristApp.API.Authentification
{
    public class JwtService
    {
        private readonly string _secret;

        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"];
        }

        public string ExtractUsername(string token)
        {
            return GetPrincipal(token).Identity.Name;
        }

        public string ExtractRole(string token)
        {
            return GetPrincipal(token)
                .Claims
                .First(x => x.Type == ClaimTypes.Role)
                .Value;
        }

        public bool IsTokenValid(string token)
        {
            try
            {
                GetPrincipal(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private ClaimsPrincipal GetPrincipal(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            return handler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_secret))
                },
                out _);
        }
    }
}
