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

        public async Task<List<HeatNetworkResponse>> GetHeatNetworkByUserId(string userId)
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksHeatNetworkByUserIdGetAsync(userId);

                if (response.IsOk)
                {
                    var networks = response.Ok();
                    _logger.LogInformation("Retrieved {Count} heat networks for user ID: {UserId}.", networks.Count, userId);
                    return networks;
                }
                else
                {
                    return new List<HeatNetworkResponse>();
                }

                throw new InvalidOperationException($"Failed to retrieve heat networks for user ID: {userId}. Status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving heat networks for user ID: {UserId}.", userId);
                throw;
            }
        }


        public async Task<HeatNetworkResponse> AddHeatNetwork(HeatNetwork heatNetwork)
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

        public async Task<HeatNetworkResponse> UpdateNetworkElements(string hnId, NetworkElements2 request)
        {
            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksNetworkElementsPutAsync(request, hnId);
                if (response.IsOk)
                {
                    var updatedNetwork = response.Ok();
                    _logger.LogInformation("Updated network elements for heat network ID: {HnId}", hnId);
                    return updatedNetwork;
                }
                throw new Exception($"Failed to update network elements for heat network '{hnId}' with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating network elements for heat network ID: {HnId}", hnId);
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

        public async Task UpdateDocument(NetworkDetailsUploadDocumentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.HnId))
                throw new ArgumentException("Heat Network ID is required.", nameof(request.HnId));

            try
            {
                var response = await _heatNetworksApi.ApiHeatNetworksNetworkDetailsDocumentUpdatePatchOrDefaultAsync(request);

                if (!response!.IsOk)
                    throw new InvalidOperationException($"Update failed with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during update for HN ID: {HnId}, UploadedBy: {UploadedBy}",
                    request.HnId, request.UploadedBy);
                throw;
            }
        }
    }
}
