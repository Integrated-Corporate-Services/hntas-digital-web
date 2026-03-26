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

        public AuditHistoryPOCController(IAuditService auditService, ILogger<AuditHistoryPOCController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        //public async Task<IActionResult> Index(string hnId)
        //{
        //    this.ShowBackButton("HeatNetworks", "UserManagement");
        //    if (string.IsNullOrWhiteSpace(hnId))
        //    {
        //        return BadRequest("Heat Network ID is required.");
        //    }

        //    var auditLogRequest = new AuditLogRequest
        //    {
        //        HnId = hnId,
        //        SortBy = null,
        //        SortDirection = "asc",
        //        Page = 1,
        //        PageSize = 20
        //    };

        //    try
        //    {
        //        // Fetch the data using the refactored method
        //        var auditLogs = await _auditService.GetAuditHistoryByHnId(auditLogRequest);

        //        // Pass the list to the View
        //        return View("Index", auditLogs);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving audit history for HN ID: {HnId}", hnId);

        //        // In a real GOV.UK app, you'd redirect to a standard "Problem with the service" page
        //        return View("Error");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> Index(
            string hnId,
            string? sortBy = "timestamp",
            string? sortOrder = "asc",
            int page = 1,
            int pageSize = 1)
        {
            try
            {
                // Validate and sanitize inputs
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100; // Max page size to prevent abuse

                // Validate sort order
                sortOrder = sortOrder?.ToLower() == "desc" ? "desc" : "asc";

                // Validate sortBy field - only allow specific fields
                var validSortFields = new[] { "EntryType", "Element" };
                //if (!string.IsNullOrEmpty(sortBy) && !validSortFields.Contains(sortBy))
                //{
                //    sortBy = null;
                //}

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
                _logger.LogError(ex, "Error retrieving audit history");
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
    }
}
