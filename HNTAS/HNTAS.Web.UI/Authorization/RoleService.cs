using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Services.Core;
using Microsoft.Extensions.Caching.Memory;

namespace HNTAS.Web.UI.Authorization
{
    public class RoleService : IRoleService
    {
        private readonly IMemoryCache _cache;
        private readonly IUserService _userService;

        public RoleService(IMemoryCache cache, IUserService userService)
        {
            _cache = cache;
            _userService = userService;
        }

        public async Task<List<UserRole>> GetRolesAsync(string oneloginId)
        {
            string cacheKey = $"roles_{oneloginId}";

            if (!_cache.TryGetValue(cacheKey, out List<UserRole> roles))
            {
                var response = await _userService.GetUserByOneLoginId(oneloginId);

                roles = response.Roles ?? new List<UserRole>();

                // Now we can set a longer time (e.g., 5 mins) because we will manually kill it when needed
                _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(5));
            }
            return roles;
        }

        public void InvalidateCache(string oneloginId)
        {
            _cache.Remove($"roles_{oneloginId}");
        }
    }
}
