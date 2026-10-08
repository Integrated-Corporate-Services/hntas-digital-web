using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HNTAS.Web.UI.Authentication
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string oneLoginId)
        {
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException("JWT_SECRET environment variable is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, oneLoginId),
            };

            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpiryHours")),
            signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
