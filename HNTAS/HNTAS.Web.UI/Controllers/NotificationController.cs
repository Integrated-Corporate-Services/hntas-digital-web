using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationHistoryService _notificationHistoryService;
        private readonly ILogger<NotificationController> _logger;
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 6;
        private const string DefaultSortBy = "timestamp";

        public NotificationController(INotificationHistoryService notificationHistoryService, ILogger<NotificationController> logger)
        {
            _notificationHistoryService = notificationHistoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string userId,
            string? sortBy = DefaultSortBy,
            string? sortOrder = "desc",
            int page = DefaultPageNumber,
            int pageSize = DefaultPageSize)
        {
            try
            {
                // Validate and sanitize inputs
                if (page < 1) page = DefaultPageNumber;

                // Validate sort order
                sortOrder = sortOrder?.ToLower() == "desc" ? "desc" : "asc";

                var validSortFields = new[] { "EntryType", "Element" };

                var notificationHistoryRequest = new NotificationHistoryRequest
                {
                    UserId = userId,
                    SortBy = sortBy,
                    SortDirection = sortOrder,
                    Page = page,
                    PageSize = pageSize
                };

                // Get audit logs with sorting and pagination
                var result = await _notificationHistoryService.GetNotificationHistory(notificationHistoryRequest);

                // Pass sorting and pagination info to view
                ViewBag.CurrentSort = sortBy;
                ViewBag.CurrentOrder = sortOrder;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = result.TotalPages ?? 1;
                ViewBag.TotalItems = result.TotalCount ?? 0;

                foreach (var item in result.Items!)
                {
                    var dictionary = new Dictionary<string, string>
                    {
                        { "hnid", item.HeatNetworkId! },
                        { "action", item.Action! },                        
                    };
                    item.ActionLink = !string.IsNullOrEmpty(item.Action)
                    ? Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                        Url.Action("ExecuteAction", "Notification")!,
                        dictionary!)
                    : null;
                    
                }

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification history for user: {userId}", SanitizeForLogging(userId));
                TempData["ErrorMessage"] = "An error occurred while retrieving the notification history.";

                // Return empty result
                var emptyResult = new NotificationHistoryResponse
                {
                    Items = new List<NotificationHistoryData>(),
                    TotalCount = 0,
                    TotalPages = 0
                };

                ViewBag.CurrentSort = sortBy;
                ViewBag.CurrentOrder = sortOrder ?? "asc";
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 0;
                ViewBag.TotalItems = 0;
                ViewBag.NextOrder = "desc";

                return View(emptyResult);
            }
        }

        public async Task<IActionResult> ExecuteAction([FromQuery] Dictionary<string, string> actionDetails)
        {
            if (actionDetails["action"] == "Heat network details")
            {
                return RedirectToAction("AddNetworkDetails", "HeatNetwork", new { hnId = actionDetails["hnid"] });
            }
            else if (actionDetails["action"] == "DDH and contributors")
            {
                return RedirectToAction("AddContributor", "UserManagement");
            }
            else if (actionDetails["action"] == "Network managers")
            {
                return RedirectToAction("ManageLeads", "NetworkLeads");
            }
            return View("ActionResult", actionDetails);
        }
        private string SanitizeForLogging(string input)
        {
            return input?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
        }


    }
}
