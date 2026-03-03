using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.NetworkDetailsUpload;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ElementSoaUploadViewModel : NetworkDetailsUploadViewModel
    {
        public SoaStage? SoaStage { get; set; }
        public string? ElementId { get; set; }
        public HeatNetworkElementDisplayType? Type { get; set; }
    }
}
