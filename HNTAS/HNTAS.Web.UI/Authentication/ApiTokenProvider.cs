using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace HNTAS.Web.UI.Authentication
{
    public class ApiTokenProvider : IApiTokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtTokenService _jwtTokenService;

        public ApiTokenProvider(IHttpContextAccessor httpContextAccessor, IJwtTokenService jwtTokenService)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string?> GetTokenAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.HntasJwt);

            if (string.IsNullOrWhiteSpace(token) || IsExpired(token))
            {
                var oneLoginId = _httpContextAccessor.HttpContext?
                    .User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(oneLoginId))
                {
                    return null;
                }

                token = _jwtTokenService.GenerateToken(oneLoginId);

                _httpContextAccessor.HttpContext?
                    .Session.SetString(SessionKeys.HntasJwt, token);
            }

            return token;
        }

        private static bool IsExpired(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();

            var token = handler.ReadJwtToken(jwt);

            return token.ValidTo <= DateTime.UtcNow;
        }
    }
}
