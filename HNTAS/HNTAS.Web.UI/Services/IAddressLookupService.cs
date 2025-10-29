using HNTAS.Web.UI.Models.Address;

namespace HNTAS.Web.UI.Services
{
    public interface IAddressLookupService
    {
        /// <summary>
        /// Retrieves a list of addresses based on the provided postcode from Ordnance Survey API.
        /// </summary>
        /// <param name="postcode">The user input postcode string</param>
        /// <returns>If no addresses are found return null, or else returns a SearchAddressByPostcodeModel model with postcode and the list of addresses in it.</returns>
        Task<SearchAddressByPostcodeModel?> PostcodeLookupAsync(string postcode);
    }
}
