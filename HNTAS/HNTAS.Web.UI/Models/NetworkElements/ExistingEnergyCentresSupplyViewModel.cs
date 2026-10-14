namespace HNTAS.Web.UI.Models.NetworkElements
{
    public class ExistingEnergyCentresSupplyViewModel
    {
        public List<ExistingEnergyCentreSupply> ExistingEnergyCentresSupply { get; set; } = new List<ExistingEnergyCentreSupply>();
    }

    public class ExistingEnergyCentreSupply
    {
        public string? EcName { get; set; } = null;
        public bool? IsChecked { get; set; } = null;
    }
}
