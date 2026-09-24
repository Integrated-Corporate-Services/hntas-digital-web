using System.Net.Http.Headers;

namespace HNTAS.Web.UI.Authentication
{
    public class ApiAuthenticationHandler : DelegatingHandler
    {
        private readonly IApiTokenProvider _tokenProvider;

        public ApiAuthenticationHandler(
            IApiTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenProvider.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
