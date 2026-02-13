
using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class AuditService : IAuditService
    {
        private readonly IAuditApi _auditApi;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IAuditApi auditApi, ILogger<AuditService> logger)
        {
            _auditApi = auditApi;
            _logger = logger;
        }

        public async Task<List<AuditLogResponse>> GetAuditHistoryByHnId(string hnId)
        {
            if (string.IsNullOrWhiteSpace(hnId))
            {
                throw new ArgumentException("Heat Network ID cannot be null or empty.", nameof(hnId));
            }

            var response = await _auditApi.ApiAuditHeatNetworkHnIdGetAsync(hnId);

            if (response.IsNotFound)
            {
                _logger.LogWarning("No audit history found for Heat Network {HnId}.", hnId);
                return new List<AuditLogResponse>();
            }

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch audit history for Heat Network {HnId}. Status code: {StatusCode}", hnId, response.StatusCode);
                throw new HttpRequestException($"Failed to fetch audit history. Service returned {response.StatusCode}");
            }


            return response.Ok() ?? new List<AuditLogResponse>();
        }
    }
}
