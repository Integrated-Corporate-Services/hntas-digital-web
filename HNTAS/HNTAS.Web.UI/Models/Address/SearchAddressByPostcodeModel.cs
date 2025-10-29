namespace HNTAS.Web.UI.Models.Address
{
    public class SearchAddressByPostcodeModel
    {
        public string? Postcode { get; set; }
        public string[]? Addresses { get; set; }
        public string? SelectedFullAddress { get; set; }
    }
}
