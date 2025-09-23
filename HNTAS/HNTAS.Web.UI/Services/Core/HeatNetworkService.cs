using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class HeatNetworkService : IHeatNetworkService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IHeatNetworksApi _heatNetworksApi;

        public HeatNetworkService(ILogger<UserService> logger, IHeatNetworksApi heatNetworksApi)
        {
            _logger = logger;
            _heatNetworksApi = heatNetworksApi;
        }

        public async Task<HeatNetworkResponse?> GetAsync(string hnId)
        {
            // _logger.LogInformation("Fetching heat network with ID: {HnId}", hnId);

            var response = await _heatNetworksApi.ApiHeatNetworksHnIdGetAsync(hnId);

            if (response.IsOk)
            {
                //_logger.LogInformation("Successfully retrieved heat network: {HnId}", hnId);
                return response.Ok();
            }
            else if (response.IsNotFound)
            {
                //_logger.LogWarning("Heat network with ID: {HnId} not found", hnId);
                return null;
            }

            //_logger.LogError("Failed to fetch heat network {HnId}. Status code: {StatusCode}", hnId, response.StatusCode);
            throw new Exception($"Failed to fetch heat network '{hnId}' — status code: {response.StatusCode}");
        }


        public async Task<HeatNetworkResponse> AddHeatNetwork(HeatNetwork heatNetwork, string hnId)
        {
            // Implementation for adding a heat network
            // _logger.LogInformation("Adding heat network: {HeatNetworkName}", heatNetwork.Name);
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksAddHeatNetworkPostAsync(heatNetwork);

                if (response.IsCreated)
                {
                    //_logger.LogInformation("Heat network created successfully with ID: {Id}", heatNetwork.Id);
                    return response.Created();
                }
                throw new Exception($"Failed to add heat network with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting heat network answers.");
                throw;
            }
        }

        public async Task<List<HeatNetworkResponse>> GetAllHeatNetworks()
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksGetAsync();

                if (response.IsOk)
                {
                    var networks = response.Ok();
                    _logger.LogInformation("Retrieved {Count} heat networks.", networks.Count);
                    return networks;
                }

                throw new InvalidOperationException($"Failed to retrieve heat networks. Status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving heat networks.");
                throw;
            }
        }
        

        public async Task<bool?> GetAssessorImpartialityAsync(string hnId)
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksHnidAssessorImpartialityGetAsync(hnId);
                if (response.IsOk)
                {
                    // Assuming the API returns a DTO with HasAssessorDeclaredImpartiality property
                    var result = response.Ok();
                    // result should have a property HasAssessorDeclaredImpartiality (bool?)
                    return result;
                }
                else if (response.IsNotFound)
                {
                    _logger.LogWarning("Impartiality status for heat network {HnId} not found", hnId);
                    return null;
                }
                throw new Exception($"Failed to get assessor impartiality for '{hnId}' — status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessor impartiality for heat network {HnId}", hnId);
                throw;
            }
        }

        public async Task<bool> SetAssessorImpartialityAsync(string hnId)
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksHnidAssessorImpartialityPostAsync(hnId);
                if (response.IsOk)
                {
                    return true;
                }
                throw new Exception($"Failed to set assessor impartiality for '{hnId}' — status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting assessor impartiality for heat network {HnId}", hnId);
                throw;
            }
        }
    }
}
