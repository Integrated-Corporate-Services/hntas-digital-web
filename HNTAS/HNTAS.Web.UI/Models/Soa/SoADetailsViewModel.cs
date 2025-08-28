namespace HNTAS.Web.UI.Models.Soa
{
    public class SoADetailsViewModel
    {
        public string HeatNetworkName { get; set; }

        public string Pathway { get; set; }

        public List<SelectedElement> SelectedElements { get; set; }

    }
    public class SelectedElement
    {
        public string Name { get; set; } = null!;

        public int Count { get; set; }
    }
}
