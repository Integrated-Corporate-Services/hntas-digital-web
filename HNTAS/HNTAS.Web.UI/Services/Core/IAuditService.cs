using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface IAuditService
    {
        Task<AuditLogResponse> GetAuditHistoryByHnId(AuditLogRequest auditLogRequest);
    }
}
