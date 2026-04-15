using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.Soa;
using System.ComponentModel.DataAnnotations;

namespace HNTAS.Web.UI.Models
{
    public class DeclationOfImpartialityModel
    {
        [Required(ErrorMessage = "You must declare your impartiality in order to progress further.")]
        public bool HasDeclaredImpartiality { get; set; }
        [Required]
        public string HnId { get; set; }
    }

    public class HeatNetworkDetailsViewModel
    {
        public string HnId;
        public string HnName;
        public AddressByStreetOrTownModel Address;
        public string OrganisationName;
        public List<SelectedElement> HeatNetworkElements { get; set; }
        public int CurrentPhaseIndex;
        public RegisteredAddress OrganisationAddress;
        public string Pathway;
        public List<PhaseViewModel> Phases { get; set; }
    }

    public class DownloadTheDocumentModel
    {
        public string Phase;
        public string Stage;
        public List<ElementItem> ElementList;
        public List<UploadedDocument> Documents;
        public List<UploadedDocument> AssessementPlan;
    }

    public class UploadSOCViewModel
    {
        public int PhaseNumber { get; set; }
        public string TemplateDownloadUrl { get; set; } = string.Empty;
    }


}
