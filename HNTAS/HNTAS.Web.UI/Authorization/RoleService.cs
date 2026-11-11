using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.Extensions.Caching.Memory;

namespace HNTAS.Web.UI.Authorization
{
    public class RoleService : IRoleService
    {
        private readonly record struct CacheKeys(string Roles, string HnRoleMapping);

        private readonly IMemoryCache _cache;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelper _sessionHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleService(
            IMemoryCache cache,
            IUserService userService,
            IConfiguration configuration,
            ISessionHelper sessionHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _userService = userService;
            _configuration = configuration;
            _sessionHelper = sessionHelper;
            _httpContextAccessor = httpContextAccessor;
        }        

        public async Task<List<UserRole>> GetRolesAsync(string oneloginId, bool isNetworkRole = false)
        {
            var cacheKeys = GetCacheKeys(oneloginId);
            List<UserRole> userRoles;
            List<HnRoleMapping> hnRoleMappings;

            if (!_cache.TryGetValue(cacheKeys.Roles, out List<UserRole> roles))
            {
                // Read from appsettings, default to 30 if not found
                var durationMinutes = _configuration.GetValue<int>("CacheSettings:RoleCacheDurationMinutes", 30);

                // Set the cache to expire strictly in 30 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(durationMinutes));

                var response = await _userService.GetUserCurrentRole(oneloginId);
                roles = response.Roles ?? new List<UserRole>();
                if (!roles.Contains(UserRole.ResponsibleParty) && response.InvitedRole.HasValue)
                {
                    roles = new List<UserRole> { MapContributorRoleToUserRole((ContributorRole)response.InvitedRole).Value };
                }

                _cache.Set(cacheKeys.HnRoleMapping, response.HnRoleMappings, cacheOptions);
                _cache.Set(cacheKeys.Roles, roles, cacheOptions);
            }

            if (isNetworkRole)
            {
                hnRoleMappings = _cache.Get<List<HnRoleMapping>>(cacheKeys.HnRoleMapping);
                var httpContext = _httpContextAccessor.HttpContext;
                var hnId = httpContext is null
                    ? null
                    : _sessionHelper.GetFromSession<string>(httpContext, SessionKeys.HnId);

                if (hnId is null)
                {
                    throw new InvalidOperationException("Heat Network ID is not available in the session.");
                }

                var hnRole = hnRoleMappings?.FirstOrDefault(mapping => mapping.HnId == hnId)?.Role;
                var mappedRole = MapContributorRoleToUserRole(hnRole);
                roles = mappedRole.HasValue
                    ? new List<UserRole> { mappedRole.Value }
                    : new List<UserRole>();
            }
            return roles;
        }

        public void ClearRoleCache(string oneloginId)
        {
            var cacheKeys = GetCacheKeys(oneloginId);
            _cache.Remove(cacheKeys.Roles);
            _cache.Remove(cacheKeys.HnRoleMapping);
        }

        private static CacheKeys GetCacheKeys(string oneloginId)
        {
            return new CacheKeys($"roles_{oneloginId}", $"hnRoleMapping_{oneloginId}");
        }

        private static UserRole? MapContributorRoleToUserRole(ContributorRole? contributorRole)
        {
            return contributorRole switch
            {
                ContributorRole.ResponsibleParty => UserRole.ResponsibleParty,
                ContributorRole.NetworkManager => UserRole.NetworkManager,
                ContributorRole.DesignatedDutyHolder => UserRole.DesignatedDutyHolder,
                ContributorRole.Contributor => UserRole.Contributor,
                ContributorRole.Assessor => UserRole.Assessor,
                ContributorRole.Certifier => UserRole.Certifier,
                _ => null
            };
        }
    }
}
