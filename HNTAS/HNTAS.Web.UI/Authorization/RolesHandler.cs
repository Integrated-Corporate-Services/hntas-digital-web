using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HNTAS.Web.UI.Authorization
{
    public class RolesHandler : AuthorizationHandler<RolesRequirement>
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesHandler> _logger;

        public RolesHandler(IRoleService roleService, ILogger<RolesHandler> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          RolesRequirement requirement)
        {
            var oneloginId = context.User.FindFirstValue("sub");
            var useGovUkSimulator = Environment.GetEnvironmentVariable("SIMULATOR_PROP4");

            if (!string.IsNullOrEmpty(useGovUkSimulator) && useGovUkSimulator.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                oneloginId = context.User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                _logger.LogInformation("Using GovUK Simulator. Extracted oneloginId from 'nameidentifier' claim: {OneLoginId}", oneloginId);
            }

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
