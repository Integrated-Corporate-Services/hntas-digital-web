using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class NetworkEcNameViewModel
    {
        public List<EcNameOption> ExistingEcNameOptions { get; set; } = new();
        public List<EcNameOption> NewEcNameOptions { get; set; } = new();
    }

    public class EcNameOption
    {
        [StringLength(100, ErrorMessage = "The energy centre name cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Za-z0-9 :;\-]+$", ErrorMessage = "The energy centre name contains invalid characters")]

        public string? EcName { get; set; } = null;
        public string? Label { get; set; } = null;
        public bool IsMainEc { get; set; } = false;
    }
}
