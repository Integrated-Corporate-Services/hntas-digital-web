using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using NuGet.Protocol.Plugins;

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
    }
}
