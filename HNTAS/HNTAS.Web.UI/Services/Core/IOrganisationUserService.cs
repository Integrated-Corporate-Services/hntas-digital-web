using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IOrganisationUserService
    {
        Task<UserResponse?> GetResponsiblePartyDetails(string OrganisationId);
    }
}
