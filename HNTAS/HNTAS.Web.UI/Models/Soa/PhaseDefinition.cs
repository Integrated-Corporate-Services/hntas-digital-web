namespace HNTAS.Web.UI.Models.Soa
{
    public class PhaseDefinition
    {
        public string Name { get; set; } = null!;
        public string Title { get; set; } = null!;
        public List<StageDefinition> Stages { get; set; } = new();
    }
}
