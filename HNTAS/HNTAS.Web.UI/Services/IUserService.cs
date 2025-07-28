using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserById(string id);
        Task<UserResponse?> GetUserByOneLoginId(string oneLoginId);
        Task<string?> CreateUser(InitialUserRegistrationRequest request);
        Task<string?> UpdateUserOrganisation(string id, UpdateOrgDetailsAndRolesRequest request);
    }
}
