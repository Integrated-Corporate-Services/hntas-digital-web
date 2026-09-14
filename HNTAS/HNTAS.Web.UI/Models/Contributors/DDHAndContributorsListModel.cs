using HNTAS.Web.UI.Models.Components;

namespace HNTAS.Web.UI.Models.Contributors
{
    public class DDHAndContributorsListModel
    {
        public string Name { get; set; }
        public string HeatNetworkId { get; set; }
        public string HeatNetworkName { get; set; }
        public string Role { get; set; }
        public InvitationStatusTag Status { get; set; }
    }
    
}
