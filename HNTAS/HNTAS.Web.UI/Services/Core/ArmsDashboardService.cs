using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Client;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class ArmsDashboardService : IArmsDashboardService
    {
        private readonly ILogger<ArmsDashboardService> _logger;
        private readonly IArmsDashboardApi _armsDashboardApi;


        public ArmsDashboardService(ILogger<ArmsDashboardService> logger, IArmsDashboardApi armsDashboardApi)
        {
            _logger = logger;
            _armsDashboardApi = armsDashboardApi;
        }

        public async Task<HeatNetworkDetailsResponse?> GetKpiNetworkDetails(string submissionId, List<string>? statusFilter = null, List<string>? typeFilter = null, int page = 1)
        {
            try
            {
                // Convert List<string> to a single comma-separated string for the API
                string statusParam = (statusFilter != null && statusFilter.Any())
                    ? string.Join(",", statusFilter)
                    : string.Empty;

                string typeParam = (typeFilter != null && typeFilter.Any())
                    ? string.Join(",", typeFilter)
                    : string.Empty;

                var response = await _armsDashboardApi.ApiArmsDashboardGetKpiNetworkDetailsGetOrDefaultAsync(
                    submissionId,
                    statusParam,
                    typeParam,
                    page);

                if (response != null && response.IsOk)
                {
                    return response.Ok();
                }

                if (response != null && response.IsNotFound)
                {
                    _logger.LogWarning("KPI Details not found for submission {submissionId}", submissionId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching KPI details for submission {submissionId}", submissionId);
                throw;
            }

            return null;
        }

        public async Task<HeatNetworkDashboardResponse?> GetKpiNetworksByRpUser(string userId, int? month, int year, int pageNumber = 1)
        {
            try
            {
                var response = await _armsDashboardApi.ApiArmsDashboardGetKpiNetworksByRpUserGetOrDefaultAsync(
                    userId,
                    month.HasValue ? month.Value : default(Option<int>),
                    year,
                    pageNumber);

                if (response != null && response.IsOk)
                {
                    return response.Ok();
                }

                if (response != null && response.IsNotFound)
                {
                    _logger.LogWarning("No networks found for RP User {userId}", userId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching KPI networks for user {userId}", userId);
                throw;
            }

            return null;
        }

        public async Task<List<KpiHistoryResponse?>> GetSubmissionHistory(string submissionId)
        {
            _logger.LogInformation("Attempting to fetch submission history for ID: {SubmissionId}", submissionId);

            try
            {
                var response = await _armsDashboardApi.ApiArmsDashboardSubmissionIdHistoryGetOrDefaultAsync(submissionId);

                // Handle success
                if (response != null && response.IsOk)
                {
                    return response.Ok();
                }

                // Handle specific failure cases
                if (response == null)
                {
                    _logger.LogError("API response was null for submission {SubmissionId}", submissionId);
                    return null;
                }

                // Log any other non-success status codes (e.g., 400, 500)
                _logger.LogError("Failed to fetch KPI history for {SubmissionId}. Status: {StatusCode}",
                    submissionId, response.StatusCode);

            }
            catch (Exception ex)
            {
                // This remains your most critical log for the screenshot error
                _logger.LogError(ex, "Exception occurred while calling GetSubmissionHistory for {SubmissionId}. Message: {Message}",
                    submissionId, ex.Message);
                throw;
            }

            return null;
        }
    }
}
