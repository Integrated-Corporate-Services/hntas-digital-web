using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Authorization
{
    public interface IRoleService
    {        
        Task<List<UserRole>> GetRolesAsync(string oneloginId, bool isNetworkRole = false);
        void ClearRoleCache(string oneloginId); // This is the "Kill Switch"
    }
}
