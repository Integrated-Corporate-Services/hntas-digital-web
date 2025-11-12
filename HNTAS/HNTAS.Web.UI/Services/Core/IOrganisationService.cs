using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IOrganisationService
    {
        Task<User?> EditOrganisationDetails(string orgId, OrganisationRequest organisationRequest, string userId);
        Task<bool?> GetOrganisationByDetails(string orgName, string postCode, string country);
    }
}
