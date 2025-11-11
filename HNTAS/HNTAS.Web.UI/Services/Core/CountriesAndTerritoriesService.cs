using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class CountriesAndTerritoriesService : ICountriesAndTerritoriesService
    {
        private readonly ILogger<CountriesAndTerritoriesService> _logger;
        private readonly ICountriesAndTerritoriesApi _countriesAndTerritoriesApi;

        public CountriesAndTerritoriesService(ILogger<CountriesAndTerritoriesService> logger, ICountriesAndTerritoriesApi countriesAndTerritoriesApi)
        {
            _logger = logger;
            _countriesAndTerritoriesApi = countriesAndTerritoriesApi;
        }


        public async Task<List<CountryAndTerritory>?> GetCountriesAndTerritories()
        {
            var response = await _countriesAndTerritoriesApi.ApiCountriesAndTerritoriesGetAsync();

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch countries and territories. Status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to fetch countries and territories with status code: {response.StatusCode}");
            }

            return response.Ok();
        }
    }
}
