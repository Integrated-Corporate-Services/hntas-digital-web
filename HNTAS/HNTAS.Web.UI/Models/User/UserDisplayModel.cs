namespace HNTAS.Web.UI.Models.User
{
    public class UserDisplayModel
    {
        public string Id { get; set; } // The ID needed for the "View" link
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Status { get; set; }
        public bool IsCurrentUser { get; set; } = false;
        public List<string>? HeatNetworks { get; set; }
        public List<string>? Roles { get; set; }
    }
}
