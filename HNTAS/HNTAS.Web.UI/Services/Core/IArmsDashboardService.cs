using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IArmsDashboardService
    {
        Task<HeatNetworkDashboardResponse?> GetKpiNetworksByRpUser(string userId, int? month, int year, int pageNumber = 1);
        Task<HeatNetworkDetailsResponse?> GetKpiNetworkDetails(string submissionId, List<string>? statusFilter = null, List<string>? typeFilter = null, int page = 1);
        Task<List<KpiHistoryResponse?>> GetSubmissionHistory(string submissionId);
    }
}
