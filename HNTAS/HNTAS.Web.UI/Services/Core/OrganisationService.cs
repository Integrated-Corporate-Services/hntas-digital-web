
using HNTAS.Api.Client.Api;

namespace HNTAS.Web.UI.Services.Core
{
    public class OrganisationService : IOrganisationService
    {
        private readonly ILogger<OrganisationService> _logger;
        private readonly IOrganisationsApi _organisationsApi;

        public OrganisationService(ILogger<OrganisationService> logger, IOrganisationsApi organisationsApi)
        {
            _logger = logger;
            _organisationsApi = organisationsApi;
        }

        public async Task<bool?> GetOrganisationByDetails(string orgName, string postCode, string country)
        {
            var response = await _organisationsApi.ApiOrganisationsExistsByDetailsGetAsync(orgName, postCode, country);
            if (response.IsOk)
            {
                return response.Ok();
            }
            throw new Exception($"Failed to fetch OrganisationByDetails with status code: {response.StatusCode}");
        }
    }
}
