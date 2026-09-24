using Microsoft.AspNetCore.Authentication;

namespace HNTAS.Web.UI.Authentication
{
    public class ApiTokenProvider : IApiTokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiTokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GetTokenAsync()
        {
            var idToken = await _httpContextAccessor.HttpContext!
                .GetTokenAsync("id_token");

            return idToken;
        }
    }
}
