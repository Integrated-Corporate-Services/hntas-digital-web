using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class NewAndExistingNetworkElementSummaryViewModel
    {
        public List<string>? ExistingNetworkElements { get; set; }
        public List<string>? NewNetworkElements { get; set; }
        public List<string>? ExistingNetworkEcNames { get; set; }
        public List<string>? NewNetworkEcNames { get; set; }
        public List<string>? ExistingEcSupplyingNewBuilding { get; set; }

        [Display(Name = "I confirm that")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Confirm that the summary is correct")]

        public bool ConfirmedDeclaration { get; set; }
    }
}
