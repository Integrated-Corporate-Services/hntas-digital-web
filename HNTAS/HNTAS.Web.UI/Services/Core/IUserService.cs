using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserById(string id);
        Task<List<HeatNetworkUserResponse>?> GetUserHeatNetworks(string id);
        Task<UserResponse?> GetUserByOneLoginId(string oneLoginId);
        Task<UserResponse?> GetUserByEmailIdAsync(string emailId);
        Task<string?> CreateUser(InitialUserRegistrationRequest request);
        Task<string?> UpdateUserOrganisation(string id, UpdateUserOrganisationRequest request);
        Task<Organisation?> UpdateOrganisationLinkUser(string userId, OrganisationRequest organisationRequest);
        Task UpdateUserWithExistingOrganisationId(string userId, string orgId);
        Task UpdateUserDetails(string id, UpdateUserDetailsRequest request);
        Task<bool?> IsOrganisationExists(string companiesHouseNumber);
        Task<List<EnumItemResponse>> GetContributorRolesAsync();
        Task<List<EnumItemResponse>> GetUserRolesAsync();
        Task<UserDetailsResponse> GetUserDetails(string userId);
        Task<List<ManagedUserResponse>> GetManagedUsers(string userId, bool networkManagersOnly = false);
        Task<List<InvitedUserResponse>> GetNetworkLeads(string userId);
        Task<List<UserResponse>> GetRegisteredUsersAsync(string rpUserId);
        Task<bool?> IsRpUserAsync(string emailId);
        Task<bool?> IsActiveUserAsync(string emailId);
        Task<List<UserRoleDetailResponse>?> GetHeatNetworkUserRoles(string heatNetworkId);
        Task<(bool IsAssigned, string UserId)> IsRoleAlreadyAssigned(string heatNetworkId, string roleName);
        Task<List<UserResponse>> GetUsersByOrganisationIdAsync(string organisationId);

        Task<bool> IsSuperUser(string emailId);
    }
}
