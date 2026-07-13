namespace HNTAS.Web.UI.Models
{
    public class DashboardModel
    {
        public string OrganisationName { get; set; } = null!;
        public bool IsResponsiblePerson { get; set; }
        public string UserRole { get; set; } = null!;

        public bool HasHeatNetworks { get; set; }
        public bool HasOfgemNetworks { get; set; }
    }
}
