using HNTAS.Api.Client.Model;
using Microsoft.AspNetCore.Authorization;

namespace HNTAS.Web.UI.Authorization
{
    public class RolesRequirement : IAuthorizationRequirement
    {
        public UserRole[] AllowedRoles { get; }
        public bool IsNetworkRole { get; }
        public RolesRequirement(bool isNetworkRole, params UserRole[] roles) 
        {
            AllowedRoles = roles;
            IsNetworkRole = isNetworkRole;
        }
    }
}
