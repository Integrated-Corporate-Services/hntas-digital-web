namespace HNTAS.Web.UI.Models
{
    public class HeatNetworkUserRolesViewModel
    {
        public string HeatNetworkName { get; set; } = null!;
        public List<UserRoles> UserRoles { get; set; } = [];
    }

    public class UserRoles
    {
        public string RoleName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string EmailId { get; set; } = null!;
    }

}