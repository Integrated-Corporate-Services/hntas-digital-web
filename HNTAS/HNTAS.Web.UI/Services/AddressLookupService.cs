using HNTAS.Web.UI.Models.Address;
using System.Text.Json;

namespace HNTAS.Web.UI.Services
{
    public class AddressLookupService : IAddressLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly ILogger<AddressLookupService> _logger;

        public AddressLookupService(HttpClient httpClient, IConfiguration config, ILogger<AddressLookupService> logger)
        {
            _httpClient = httpClient;
            _apiKey = Environment.GetEnvironmentVariable("OS_API_KEY");
            _logger = logger;
        }

        public async Task<SearchAddressByPostcodeModel?> PostcodeLookupAsync(string postcode)
        {
            _logger.LogInformation("OS_Apikey : {_apiKey}", _apiKey);
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.os.uk/search/places/v1/postcode?postcode={postcode}&key={_apiKey}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("results", out var results))
            {
                return null;
            }

            var addressesInResults = doc.RootElement.GetProperty("results").EnumerateArray();

            // Simplify collection initialization
            var addresses = new List<string>();

            foreach (var addressResult in addressesInResults)
            {
                // Get the DPA property which contains the address details
                if (addressResult.TryGetProperty("DPA", out var dpa) && dpa.TryGetProperty("ADDRESS", out var addressElement))
                {
                    // Get the address as a string and add to the list
                    var addressString = addressElement.GetString();
                    if (!string.IsNullOrEmpty(addressString))
                    {
                        addresses.Add(addressString);
                    }
                }
            }

            string[] addressesArray = addresses.ToArray() ?? [];

            return new SearchAddressByPostcodeModel { Postcode = postcode, Addresses = addressesArray };
        }
    }
}
