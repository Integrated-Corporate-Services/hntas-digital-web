using HNTAS.Api.Client.Model;
using Microsoft.AspNetCore.Authorization;

namespace HNTAS.Web.UI.Authorization
{
    public class RolesRequirement : IAuthorizationRequirement
    {
        public UserRole[] AllowedRoles { get; }
        public RolesRequirement(params UserRole[] roles) => AllowedRoles = roles;
    }
}
