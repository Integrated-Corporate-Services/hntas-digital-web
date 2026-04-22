using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;

namespace HNTAS.Web.UI.Services.Core
{
    public class NotificationHistoryService : INotificationHistoryService
    {
        private readonly INotificationHistoryApi _notificationHistoryApi;
        private readonly ILogger<NotificationHistoryService> _logger;

        public NotificationHistoryService(INotificationHistoryApi notificationHistoryApi, ILogger<NotificationHistoryService> logger)
        {
            _notificationHistoryApi = notificationHistoryApi;
            _logger = logger;
        }

        public async Task<NotificationHistoryResponse> GetNotificationHistory(NotificationHistoryRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(request.UserId));
            }

            var response = await _notificationHistoryApi.ApiNotificationHistoryNotificationHistoryGetAsync(request);

            if (response.IsNotFound)
            {
                _logger.LogWarning("No notification history found for User ID {UserId}.", SanitizeForLogging(request.UserId));
                return new NotificationHistoryResponse();
            }

            if (!response.IsOk)
            {
                _logger.LogError("Failed to fetch notification history for UserId {UserId}. Status code: {StatusCode}", SanitizeForLogging(request.UserId), response.StatusCode);
                throw new HttpRequestException($"Failed to fetch notification history. Service returned {response.StatusCode}");
            }

            return response.Ok() ?? new NotificationHistoryResponse();
        }

        private string SanitizeForLogging(string input)
        {
            return input?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
        }
    }
}
