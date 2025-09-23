using HNTAS.Api.Client.Model;
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
        public string HnLocation;
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
        public string Element;
        public List<ElementViewModel> ElementList;
        public List<UploadedDocument> Documents;
        public DocumentItem AssessementPlan;
    }

    public class UploadSOCViewModel
    {
        public int PhaseNumber { get; set; }
        public string TemplateDownloadUrl { get; set; } = string.Empty;
    }

    public class AssessorCYAModel
    {
        public string ElementName { get; set; }
        public string PhaseName { get; set; }
        public string StageName { get; set; }
        public string SOCfileName { get; set; }

    }

    public class SOCSubmittedModel
    {
        public string ElementName { get; set; }
        
    }
}
