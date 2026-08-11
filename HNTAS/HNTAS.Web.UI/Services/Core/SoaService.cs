using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class SoaService : ISoaService
    {
        private readonly ISOAApi _soaApi;
        private readonly ILogger<SoaService> _logger;

        public SoaService(ISOAApi soaProjectApi, ILogger<SoaService> logger)
        {
            _soaApi = soaProjectApi;
            _logger = logger;
        }           

        public async Task UpdateElementSoaStatus(ElementSoaStatusUpdateRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.HnId))
                throw new ArgumentException("Heat Network ID is required.", nameof(request.HnId));

            try
            {
                var response = await _soaApi.ApiSOAUpdateSoaStatusPatchAsync(request);

                if (!response.IsOk)
                    throw new InvalidOperationException($"Soa status update failed with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during soa status update for HN ID: {HnId}, Element:{ElementId}, Stage: {Stage}, UpdatedBy: {UpdatedBy}",
                SanitizeForLogging(request.HnId), SanitizeForLogging(request.ElementId!), request.Stage, SanitizeForLogging(request.SoaStatusUpdatedBy!));
                throw;
            }
        }

        public async Task UpdateElementSoaStatusForExistingNetwork(ElementSoaStatusUpdateRequestForExistingNetwork request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.HnId))
                throw new ArgumentException("Heat Network ID is required.", nameof(request.HnId));

            try
            {
                var response = await _soaApi.ApiSOAUpdateSoaStatusForExistingNetworkPatchAsync(request);

                if (!response.IsOk)
                    throw new InvalidOperationException($"Soa status update failed with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during soa status update for HN ID: {HnId}, Element:{ElementId}, Milestone: {Milestone}, UpdatedBy: {UpdatedBy}",
                SanitizeForLogging(request.HnId), SanitizeForLogging(request.ElementId!), request.Milestone, SanitizeForLogging(request.SoaStatusUpdatedBy!));
                throw;
            }
        }

        public async Task AssignAssessor(ElementSoaAssignAssessorRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.HnId))
                throw new ArgumentException("Heat Network ID is required.", nameof(request.HnId));

            try
            {
                var response = await _soaApi.ApiSOASoaAssignAssessorPatchAsync(request);

                if (!response.IsOk)
                    throw new InvalidOperationException($"Soa status update failed with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during saving Assessor Assigned for HN ID: {HnId}, UpdatedBy: {UpdatedBy}",
                SanitizeForLogging(request.HnId), SanitizeForLogging(request.UpdatedBy!));
                throw;
            }
        }

        public async Task AssignAssessorForExistingNetwork(ElementSoaAssignAssessorRequestForExistingNetwork request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.HnId))
                throw new ArgumentException("Heat Network ID is required.", nameof(request.HnId));

            try
            {
                var response = await _soaApi.ApiSOASoaAssignAssessorForExistingNetworkPatchAsync(request);

                if (!response.IsOk)
                    throw new InvalidOperationException($"Soa status update failed with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during saving Assessor Assigned for HN ID: {HnId}, UpdatedBy: {UpdatedBy}",
                SanitizeForLogging(request.HnId), SanitizeForLogging(request.UpdatedBy!));
                throw;
            }
        }

        private string SanitizeForLogging(string input)
        {
            return input?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
        }
    }
}
