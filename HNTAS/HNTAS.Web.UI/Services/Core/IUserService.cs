using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserById(string id);

        Task<List<HeatNetwork>?> GetUserHeatNetworks(string id);

        Task<UserResponse?> GetUserByOneLoginId(string oneLoginId);

        Task<string?> CreateUser(InitialUserRegistrationRequest request);

        Task<string?> UpdateUserOrganisation(string id, UpdateUserOrganisationRequest request);

        Task UpdateUserHeatNetworkId(string id, string heatNetworkId);

        Task<bool?> IsOrganisationExists(string companiesHouseNumber);

        Task<List<EnumItemResponse>> GetContributorRolesAsync();
        Task<List<EnumItemResponse>> GetUserRolesAsync();


        Task<UserDetailsResponse> GetUserDetails(string userId);

        Task<ManagedUserResponse> GetManagedUsers(string userId);

        Task<string?> AcceptUserInvitation(InvitedUserRequest userRequest);

        Task<List<UserResponse>> GetRegisteredUsersAsync(string rpUserId);
    }
}
