using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.Cache
{
    public class UserCacheModel
    {
        public string UserId { get; set; } = string.Empty;

        public string OneLoginId { get; set; } = string.Empty;

        public List<UserRole> Roles { get; set; } = [];
    }
}
