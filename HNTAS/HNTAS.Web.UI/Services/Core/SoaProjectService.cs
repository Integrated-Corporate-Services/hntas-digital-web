using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class SoaProjectService : ISoaProjectService
    {
        private readonly ISoaProjectApi _soaProjectApi;
        private readonly ILogger<SoaProjectService> _logger;

        public SoaProjectService(ISoaProjectApi soaProjectApi, ILogger<SoaProjectService> logger)
        {
            _soaProjectApi = soaProjectApi;
            _logger = logger;
        }

        public async Task<SoaProject> GetAsync(string projectId)
        {
            _logger.LogInformation("Fetching SOA project with ID: {ProjectId}", projectId);

            var response = await _soaProjectApi.ApiSoaProjectProjectIdGetAsync(projectId);

            if (response.IsOk)
            {
                _logger.LogInformation("SOA project retrieved successfully for ID: {ProjectId}", projectId);
                return response.Ok();
            }

            _logger.LogError("Failed to fetch SOA project. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, projectId);
            throw new Exception($"Failed to fetch SoaProject with status code: {response.StatusCode}");
        }

        public async Task<SoaProject> GetByHnIdAsync(string hnId)
        {
            _logger.LogInformation("Fetching SOA project with ID: {ProjectId}", hnId);

            var response = await _soaProjectApi.ApiSoaProjectHeatNetworkHnIdGetAsync(hnId);

            if (response.IsOk)
            {
                _logger.LogInformation("SOA project retrieved successfully for ID: {ProjectId}", hnId);
                return response.Ok();
            }
            else if (response.IsNotFound)
            {
                return null;
            }

            _logger.LogError("Failed to fetch SOA project. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, hnId);
            throw new Exception($"Failed to fetch SoaProject with status code: {response.StatusCode}");
        }

        public async Task<SoaProject> CreateAsync(string hnId)
        {
            _logger.LogInformation("Creating new SOA project for Heat Network ID: {HeatNetworkId}", hnId);

            var response = await _soaProjectApi.ApiSoaProjectCreatePostAsync(hnId);

            if (response.IsCreated)
            {
                var createdProject = response.Created();
                _logger.LogInformation("SOA project created successfully with ID: {ProjectId}", createdProject.Id);
                return createdProject;
            }

            _logger.LogError("Failed to create SOA project. Status code: {StatusCode}, Heat Network ID: {HeatNetworkId}", response.StatusCode, hnId);
            throw new Exception($"Failed to fetch SoaProject with status code: {response.StatusCode}");
        }

        public async Task UpdateNetworkTypeAsync(string hnId, NetworkTypeSelection2 networkTypeSelection)
        {
            _logger.LogInformation("Updating network type for project ID: {ProjectId} to {NetworkType}", hnId, networkTypeSelection.Type);

            var response = await _soaProjectApi.ApiSoaProjectNetworkTypePatchAsync(networkTypeSelection, hnId);

            if (response.IsOk)
            {
                _logger.LogInformation("Network type updated successfully for project ID: {ProjectId}", hnId);
                return;
            }

            _logger.LogError("Failed to update network type. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, hnId);
            throw new Exception($"Failed to update SoaProject with status code: {response.StatusCode}");
        }

        public async Task UpdateConnectionsAsync(string hnId, List<ConnectionType> connectionTypes)
        {
            _logger.LogInformation("Updating connection types for project ID: {ProjectId} with values: {ConnectionTypes}", hnId, string.Join(", ", connectionTypes));

            var request = new UpdateConnectionsRequest(hnId, connectionTypes);
            var response = await _soaProjectApi.ApiSoaProjectConnectionsPatchAsync(request);

            if (response.IsOk)
            {
                _logger.LogInformation("Connection types updated successfully for project ID: {ProjectId}", hnId);
                return;
            }

            _logger.LogError("Failed to update connection types. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, hnId);
            throw new Exception($"Failed to update SoaProject with status code: {response.StatusCode}");
        }
    }
}
