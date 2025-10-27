namespace HNTAS.Web.UI.Models
{
    public class DashboardModel
    {
        public string OrganisationName { get; set; } = null!;
        public bool IsRegulatoryContact { get; set; }
        public string UserRole { get; set; } = null!;

        public bool HasHeatNetworks { get; set; }
    }
}
