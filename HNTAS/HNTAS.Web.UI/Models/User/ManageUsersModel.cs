namespace HNTAS.Web.UI.Models.User
{
    public class ManageUsersModel
    {
        public string OrganisationName { get; set; }
        public List<UserDisplayModel> Users { get; set; } = new List<UserDisplayModel>();
    }
}
