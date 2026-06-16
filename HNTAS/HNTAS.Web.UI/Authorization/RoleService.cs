using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Services.Core;
using Microsoft.Extensions.Caching.Memory;

namespace HNTAS.Web.UI.Authorization
{
    public class RoleService : IRoleService
    {
        private readonly IMemoryCache _cache;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public RoleService(IMemoryCache cache, IUserService userService, IConfiguration configuration)
        {
            _cache = cache;
            _userService = userService;
            _configuration = configuration;
        }

        public async Task<List<UserRole>> GetRolesAsync(string oneloginId)
        {
            string cacheKey = $"roles_{oneloginId}";

            if (!_cache.TryGetValue(cacheKey, out List<UserRole> roles))
            {
                var response = await _userService.GetUserByOneLoginId(oneloginId);

                roles = response.Roles ?? new List<UserRole>();

                // Read from appsettings, default to 30 if not found
                var durationMinutes = _configuration.GetValue<int>("CacheSettings:RoleCacheDurationMinutes", 30);

                // Set the cache to expire strictly in 30 minutes
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(durationMinutes));

                _cache.Set(cacheKey, roles, cacheOptions);
            }
            return roles;
        }

        public void ClearRoleCache(string oneloginId)
        {
            _cache.Remove($"roles_{oneloginId}");
        }
    }
}
