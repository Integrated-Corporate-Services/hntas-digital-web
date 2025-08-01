using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserById(string id);

        Task<UserResponse?> GetUserByOneLoginId(string oneLoginId);

        Task<string?> CreateUser(InitialUserRegistrationRequest request);

        Task<string?> UpdateUserOrganisation(string id, UpdateOrgDetailsAndRolesRequest request);

        Task UpdateUserHeatNetworkId(string id, string heatNetworkId);

        Task<bool?> IsOrganisationHasRpUser(string companiesHouseNumber);
    }
}
