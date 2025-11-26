
using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

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

        public async Task<User?> EditOrganisationDetails(string orgId, OrganisationRequest organisationRequest, string userId)
        {
            try
            {
                var response = await _organisationsApi.ApiOrganisationsOrgIdEditOrgDetailsPatchOrDefaultAsync(orgId, organisationRequest, userId);
                if (response.IsOk)
                {
                    return response.Ok();
                }
                else if (response.IsNotFound)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organisation with ID {orgId}", orgId);
                throw;
            }
            return null;
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

        public async Task<Organisation?> GetOrganisationByIdOrName(string searchTerm)
        {
            var response = await _organisationsApi.ApiOrganisationsSearchGetAsync(searchTerm);
            if (response.IsOk)
            {
                return response.Ok();
            }
            else if (response.IsNotFound)
            {
                return null;
            }
            throw new Exception($"Failed to fetch OrganisationIdOrName with status code: {response.StatusCode}");
        }

        public async Task<Organisation?> GetOrganisationById(string orgId)
        {
            var response = await _organisationsApi.ApiOrganisationsOrgIdGetAsync(orgId);
            if (response.IsOk)
            {
                return response.Ok();
            }
            else if (response.IsNotFound)
            {
                return null;
            }
            throw new Exception($"Failed to fetch OrganisationById with status code: {response.StatusCode}");
        }
    }
}
