using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.Soa
{
    public class StageDefinition
    {
        public string Name { get; set; } = null!;
        public SoaStage SoaStage { get; set; }
        public List<ElementViewModel> ElementList { get; set; } = [];
    }
}
