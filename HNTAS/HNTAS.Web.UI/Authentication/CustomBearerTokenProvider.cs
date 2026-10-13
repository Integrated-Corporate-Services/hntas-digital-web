using HNTAS.Api.Client;
using HNTAS.Api.Client.Client;

namespace HNTAS.Web.UI.Authentication
{
    public class CustomBearerTokenProvider : TokenProvider<BearerToken>
    {
        private readonly IApiTokenProvider _tokenProvider;

        public CustomBearerTokenProvider(IApiTokenProvider tokenProvider)
            : base(new[] { new BearerToken(string.Empty) })
        {
            _tokenProvider = tokenProvider;
        }

        // Paste your logic into the auto-generated method signature:
        protected internal override async ValueTask<BearerToken> GetAsync(string header, CancellationToken cancellation)
        {
            var token = await _tokenProvider.GetTokenAsync();
    
            return new BearerToken(token ?? string.Empty);
        }
    }
}
