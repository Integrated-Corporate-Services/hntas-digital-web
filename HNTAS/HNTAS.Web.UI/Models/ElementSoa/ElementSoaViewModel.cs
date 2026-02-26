using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Models.ElementSoa
{
    public class ElementSoaViewModel
    {
        public NetworkDetailsStatus Status { get; set; }
        public List<SoaStagesView> Stages { get; set; } = [];
        public int CurrentStageIndex { get; set; } = 0;
    }

    public class SoaStagesView : SoaStages
    {
        public List<Elements> Elements { get; set; } = [];
        public bool IsActive { get; set; }
        public string Name { get; set; }
    }
}
