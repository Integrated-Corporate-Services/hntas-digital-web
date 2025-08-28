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

        public async Task<HeatNetwork> AddHeatNetwork(HeatNetwork heatNetwork, string hnId)
        {
            // Implementation for adding a heat network
            _logger.LogInformation("Adding heat network: {HeatNetworkName}", heatNetwork.Name);
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksAddHeatNetworkPostAsync(heatNetwork);

                if (response.IsCreated)
                {
                    _logger.LogInformation("Heat network created successfully with ID: {Id}", heatNetwork.Id);
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
    }
}
