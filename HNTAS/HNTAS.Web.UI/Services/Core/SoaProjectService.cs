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
            //_logger.LogInformation("Fetching SOA project with ID: {ProjectId}", projectId);

            var response = await _soaProjectApi.ApiSoaProjectProjectIdGetAsync(projectId);

            if (response.IsOk)
            {
                //_logger.LogInformation("SOA project retrieved successfully for ID: {ProjectId}", projectId);
                return response.Ok();
            }

            //_logger.LogError("Failed to fetch SOA project. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, projectId);
            throw new Exception($"Failed to fetch SoaProject with status code: {response.StatusCode}");
        }

        public async Task<SoaProject> GetByHnIdAsync(string hnId)
        {
            //_logger.LogInformation("Fetching SOA project with ID: {ProjectId}", hnId);

            var response = await _soaProjectApi.ApiSoaProjectHeatNetworkHnIdGetAsync(hnId);

            if (response.IsOk)
            {
                //_logger.LogInformation("SOA project retrieved successfully for ID: {ProjectId}", hnId);
                return response.Ok();
            }
            else if (response.IsNotFound)
            {
                return null;
            }

            // _logger.LogError("Failed to fetch SOA project. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, hnId);
            throw new Exception($"Failed to fetch SoaProject with status code: {response.StatusCode}");
        }

        public async Task<SoaProject> CreateAsync(string hnId, string createdBy)
        {
            //_logger.LogInformation("Creating new SOA project for Heat Network ID: {HeatNetworkId} by {CreatedBy}", hnId, createdBy);

            var response = await _soaProjectApi.ApiSoaProjectCreatePostAsync(hnId, createdBy);

            if (response.IsCreated)
            {
                var createdProject = response.Created();
                //_logger.LogInformation("SOA project created successfully with ID: {ProjectId} for Heat Network ID: {HeatNetworkId} by {CreatedBy}", createdProject.Id, hnId, createdBy);
                return createdProject;
            }

            //_logger.LogError("Failed to create SOA project. Status code: {StatusCode}, Heat Network ID: {HeatNetworkId}, CreatedBy: {CreatedBy}", response.StatusCode, hnId, createdBy);
            throw new Exception($"Failed to create SoaProject for HN ID '{hnId}' by '{createdBy}' — status code: {response.StatusCode}");
        }


        public async Task UpdateNetworkTypeAsync(string hnId, string updatedBy, NetworkTypeSelection2 networkTypeSelection)
        {
            //_logger.LogInformation("Updating network type for project ID: {ProjectId} to {NetworkType}", hnId, networkTypeSelection.Type);

            var response = await _soaProjectApi.ApiSoaProjectNetworkTypePatchAsync(networkTypeSelection, hnId, updatedBy);

            if (response.IsOk)
            {
                // _logger.LogInformation("Network type updated successfully for project ID: {ProjectId}", hnId);
                return;
            }

            //_logger.LogError("Failed to update network type. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, hnId);
            throw new Exception($"Failed to update SoaProject with status code: {response.StatusCode}");
        }

        public async Task UpdateConnectionsAsync(string hnId, string updatedBy, List<ConnectionType> connectionTypes)
        {
            // _logger.LogInformation("Updating connection types for project ID: {ProjectId} with values: {ConnectionTypes}", hnId, string.Join(", ", connectionTypes));

            var request = new UpdateConnectionsRequest(hnId, updatedBy, connectionTypes);
            var response = await _soaProjectApi.ApiSoaProjectConnectionsPatchAsync(request);

            if (response.IsOk)
            {
                //_logger.LogInformation("Connection types updated successfully for project ID: {ProjectId}", hnId);
                return;
            }

            // _logger.LogError("Failed to update connection types. Status code: {StatusCode}, Project ID: {ProjectId}", response.StatusCode, hnId);
            throw new Exception($"Failed to update SoaProject with status code: {response.StatusCode}");
        }

        public async Task UpdateNetworkElements(string hnId, string updatedBy, List<HeatNetworkElement> networkElements)
        {
            //_logger.LogInformation("Updating network elements for Heat Network ID: {HnId} by {UpdatedBy}. Element count: {ElementCount}", hnId, updatedBy, networkElements?.Count ?? 0);

            try
            {
                var response = await _soaProjectApi.ApiSoaProjectNetworkElementsPatchAsync(networkElements, hnId, updatedBy);

                if (response.IsOk)
                {
                    // _logger.LogInformation("Network elements updated successfully for Heat Network ID: {HnId} by {UpdatedBy}", hnId, updatedBy);
                }
                else
                {
                    //_logger.LogWarning("Network element update returned non-OK status. Status code: {StatusCode}, HN ID: {HnId}, UpdatedBy: {UpdatedBy}", response.StatusCode, hnId, updatedBy);
                    throw new Exception($"Update failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating network elements for Heat Network ID: {HnId} by {UpdatedBy}", hnId, updatedBy);
                throw;
            }
        }


        public async Task UpdateElementLocations(UpdateElementLocationsRequest request)
        {
            //_logger.LogInformation("Updating element locations for HN ID: {HnId}, ElementType: {ElementType}, UpdatedBy: {UpdatedBy}. Location count: {LocationCount}",
            //  request.HnId, request.ElementType, request.UpdatedBy, request.Locations?.Count ?? 0);

            try
            {
                var response = await _soaProjectApi.ApiSoaProjectElementLocationsPostAsync(request);

                if (response.IsOk)
                {
                    //_logger.LogInformation("Element locations updated successfully for HN ID: {HnId}, ElementType: {ElementType}", request.HnId, request.ElementType);
                }
                else
                {
                    // _logger.LogWarning("Element location update returned non-OK status. StatusCode: {StatusCode}, HN ID: {HnId}, ElementType: {ElementType}",
                    //    response.StatusCode, request.HnId, request.ElementType);
                    throw new Exception($"Update failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating element locations for HN ID: {HnId}, ElementType: {ElementType}", request.HnId, request.ElementType);
                throw;
            }
        }

        public async Task UpdateElementDocuments(UpdateElementDocumentsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.HnId) || request.Documents == null)
            {
                // _logger.LogWarning("Invalid document update request: {@Request}", request);
                throw new ArgumentException("Request is missing required fields.");
            }

            //  _logger.LogInformation("Initiating document update for HN ID: {HnId}, ElementType: {ElementType}, UpdatedBy: {UpdatedBy}. Document count: {Count}",
            //      request.HnId, request.ElementType, request.UpdatedBy, request.Documents.Count);

            try
            {
                var response = await _soaProjectApi.ApiSoaProjectElementDocumentsPostAsync(request);

                if (response.IsOk)
                {
                    //_logger.LogInformation("Documents updated successfully for HN ID: {HnId}, ElementType: {ElementType}, UpdatedBy: {UpdatedBy}",
                    //    request.HnId, request.ElementType, request.UpdatedBy);
                }
                else
                {
                    // _logger.LogWarning("Document update failed with status code: {StatusCode}. HN ID: {HnId}, ElementType: {ElementType}, UpdatedBy: {UpdatedBy}",
                    //     response.StatusCode, request.HnId, request.ElementType, request.UpdatedBy);
                    throw new Exception($"Document update failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during document update for HN ID: {HnId}, ElementType: {ElementType}, UpdatedBy: {UpdatedBy}",
                    request.HnId, request.ElementType, request.UpdatedBy);
                throw;
            }
        }


    }
}
