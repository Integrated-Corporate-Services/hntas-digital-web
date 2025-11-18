using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserById(string id);

        Task<List<HeatNetworkResponse>?> GetUserHeatNetworks(string id);

        Task<UserResponse?> GetUserByOneLoginId(string oneLoginId);

        Task<string?> CreateUser(InitialUserRegistrationRequest request);

        Task<string?> UpdateUserOrganisation(string id, UpdateUserOrganisationRequest request);

        Task UpdateUserHeatNetworkId(string id, string heatNetworkId);

        Task<bool?> IsOrganisationExists(string companiesHouseNumber);

        Task<List<EnumItemResponse>> GetContributorRolesAsync();
        Task<List<EnumItemResponse>> GetUserRolesAsync();


        Task<UserDetailsResponse> GetUserDetails(string userId);

        Task<List<ManagedUserResponse>> GetManagedUsers(string userId);

        Task<string?> AcceptUserInvitation(InvitedUserRequest userRequest);

        Task<List<UserResponse>> GetRegisteredUsersAsync(string rpUserId);

        Task<bool?> IsRpUserAsync(string emailId);
        Task<bool?> IsActiveUserAsync(string emailId);

        Task<List<UserRoleDetailResponse>?> GetHeatNetworkUserRoles(string heatNetworkId);
        Task<(bool IsAssigned, string UserId)> IsRoleAlreadyAssigned(string heatNetworkId, string roleName);
    }
}
