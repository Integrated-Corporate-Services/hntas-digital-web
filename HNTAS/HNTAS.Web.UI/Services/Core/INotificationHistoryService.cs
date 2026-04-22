using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public interface INotificationHistoryService
    {
        Task<NotificationHistoryResponse> GetNotificationHistory(NotificationHistoryRequest request);
    }
}
