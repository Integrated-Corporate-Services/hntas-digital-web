using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class EnergyCentrePlantDataService : IEnergyCentrePlantDataService
    {
        private readonly ILogger<EnergyCentrePlantDataService> _logger;
        private readonly IChpOperationalModesApi _chpOperationalModesApi;
        private readonly IFuelTypesApi _fuelTypesApi;
        private readonly IHeatRecoverySourcesApi _heatRecoverySourcesApi;

        public EnergyCentrePlantDataService(ILogger<EnergyCentrePlantDataService> logger, IChpOperationalModesApi chpOperationalModesApi, IFuelTypesApi fuelTypesApi, IHeatRecoverySourcesApi heatRecoverySourcesApi)
        {
            _logger = logger;
            _chpOperationalModesApi = chpOperationalModesApi;
            _fuelTypesApi = fuelTypesApi;
            _heatRecoverySourcesApi = heatRecoverySourcesApi;
        }


        public async Task<List<FuelType>?> GetFuelTypes()
        {
            var response = await _fuelTypesApi.ApiFuelTypesGetAsync();

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch fuel types. Status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to fetch fuel types with status code: {response.StatusCode}");
            }

            return response.Ok();
        }

        public async Task<List<CHPOperationalMode>?> GetChpOperationalModes()
        {
            var response = await _chpOperationalModesApi.ApiChpOperationalModesGetAsync();

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch CHP operational modes. Status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to fetch CHP operational modes with status code: {response.StatusCode}");
            }

            return response.Ok();
        }

        public async Task<List<HeatRecovery>?> GetHeatRecoverySources()
        {
            var response = await _heatRecoverySourcesApi.ApiHeatRecoverySourcesGetAsync();

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch heat recovery sources. Status code: {StatusCode}", response.StatusCode);
                throw new Exception($"Failed to fetch heat recovery sources with status code: {response.StatusCode}");
            }

            return response.Ok();
        }
    }
}
