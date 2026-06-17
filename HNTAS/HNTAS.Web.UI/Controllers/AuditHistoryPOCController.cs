using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class AuditHistoryPOCController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditHistoryPOCController> _logger;
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 6;
        private const string DefaultSortBy = "timestamp";

        public AuditHistoryPOCController(IAuditService auditService, ILogger<AuditHistoryPOCController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }        

        [HttpGet]
        public async Task<IActionResult> Index(
            string hnId,
            string? sortBy = DefaultSortBy,
            string? sortOrder = "asc",
            int page = DefaultPageNumber,
            int pageSize = DefaultPageSize)
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            try
            {
                // Validate and sanitize inputs
                if (page < 1) page = DefaultPageNumber;                

                // Validate sort order
                sortOrder = sortOrder?.ToLower() == "desc" ? "desc" : "asc";

                var auditLogRequest = new AuditLogRequest
                {
                    HnId = hnId.ToUpper(),
                    SortBy = sortBy,
                    SortDirection = sortOrder,
                    Page = page,
                    PageSize = pageSize
                };

                // Get audit logs with sorting and pagination
                var result = await _auditService.GetAuditHistoryByHnId(auditLogRequest);

                // Pass sorting and pagination info to view
                ViewBag.CurrentSort = sortBy;
                ViewBag.CurrentOrder = sortOrder;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = result.TotalPages ?? 1;
                ViewBag.TotalItems = result.TotalCount ?? 0;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit history for HN ID: {HnId}", SanitizeForLogging(hnId));
                TempData["ErrorMessage"] = "An error occurred while retrieving the certification history.";

                // Return empty result
                var emptyResult = new AuditLogResponse
                {
                    Items = new List<AuditLog>(),
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
        private string SanitizeForLogging(string input)
        {
            return input?.Replace("\r", "").Replace("\n", "") ?? string.Empty;
        }
    }
}
