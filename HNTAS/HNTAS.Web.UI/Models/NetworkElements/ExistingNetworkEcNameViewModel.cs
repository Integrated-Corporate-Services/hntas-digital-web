namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class ExistingNetworkEcNameViewModel
    {
        public List<EcNameOption> ExistingEcNameOptions { get; set; } = new();
        public List<EcNameOption> NewEcNameOptions { get; set; } = new();
    }

    public class EcNameOption
    {
        public string? EcName { get; set; } = null;
        public string? Label { get; set; } = null;
        public bool IsMainEc { get; set; } = false;
    }
}
