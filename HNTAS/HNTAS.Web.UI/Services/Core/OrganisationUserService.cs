using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class OrganisationUserService : IOrganisationUserService
    {
        private readonly ILogger<OrganisationUserService> _logger;
        private readonly IOrganisationUserApi _organisationUserApi;

        public OrganisationUserService(ILogger<OrganisationUserService> logger,
            IOrganisationUserApi organisationUserApi)
        {
            _logger = logger;
            _organisationUserApi = organisationUserApi;
        }

        public async Task<UserResponse?> GetResponsiblePartyDetails(string OrganisationId)
        {
            var response = await _organisationUserApi.ApiOrganisationUserResponsiblePartyUserOrgIdGetAsync(OrganisationId.ToUpper());
            if (response.IsOk)
            {
                return response.Ok();
            }
            else if (response.IsNotFound)
            {
                _logger.LogWarning("No Responsible Party found for OrganisationId: {OrganisationId}", OrganisationId);
                return null;
            }
            else
            {
                _logger.LogError("Error retrieving Responsible Party for OrganisationId: {OrganisationId}. Status Code: {StatusCode}", OrganisationId, response.StatusCode);
                throw new ApplicationException($"Error retrieving Responsible Party details. Status Code: {response.StatusCode}");
            }
        }
    }
}
