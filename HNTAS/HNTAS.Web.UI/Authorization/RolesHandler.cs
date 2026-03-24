using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HNTAS.Web.UI.Authorization
{
    public class RolesHandler : AuthorizationHandler<RolesRequirement>
    {
        private readonly IRoleService _roleService;

        public RolesHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        protected override async Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          RolesRequirement requirement)
        {
            var oneloginId = context.User.FindFirstValue("sub");

            if (oneloginId == null) return;

            // Get roles from your "Single Source of Truth" (the DB)
            var userRoles = await _roleService.GetRolesAsync(oneloginId);

            // LOGIC:
            // 1. If AllowedRoles is empty, we just care that the user exists in our system.
            // 2. If AllowedRoles has items, the user MUST have at least one of them.
            bool isAllowed = !requirement.AllowedRoles.Any() ||
                             requirement.AllowedRoles.Any(r => userRoles.Contains(r));

            if (isAllowed)
            {
                context.Succeed(requirement);
            }
        }
    }
}
