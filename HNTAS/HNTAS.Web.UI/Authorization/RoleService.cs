using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Cache;
using HNTAS.Web.UI.Services.Core;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HNTAS.Web.UI.Authorization
{
    public class RoleService : IRoleService
    {
        private readonly IDistributedCache _cache;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public RoleService(IDistributedCache cache, IUserService userService, IConfiguration configuration)
        {
            _cache = cache;
            _userService = userService;
            _configuration = configuration;
        }

        public async Task<List<UserRole>> GetRolesAsync(string oneLoginId)
        {
            string cacheKey = $"user:{oneLoginId}";

            var cachedUser = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedUser))
            {
                var cachedUserModel = JsonSerializer.Deserialize<UserCacheModel>(cachedUser);

                return cachedUserModel?.Roles ?? [];
            }

            var response = await _userService.GetUserByOneLoginId(oneLoginId);

            var userCache = new UserCacheModel
            {
                UserId = response.Id,
                OneLoginId = oneLoginId,
                Roles = response.Roles ?? []
            };

            var durationMinutes = _configuration.GetValue<int>(
                "CacheSettings:RoleCacheDurationMinutes",
                30);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(durationMinutes)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(userCache),
                options);

            return userCache.Roles;
        }

        public void ClearRoleCache(string oneLoginId)
        {
            _cache.Remove($"user:{oneLoginId}");
        }
    }
}
