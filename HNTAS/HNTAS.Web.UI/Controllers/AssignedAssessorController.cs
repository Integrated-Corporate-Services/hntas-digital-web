using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Controllers
{
    public class AssignedAssessorController : Controller
    {
        private readonly ILogger<AssignedAssessorController> _logger;
        private readonly IAssignedAssessorService _assignedAssessorService;
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 6;
        private const string DefaultSortBy = "assessorupdatedat";

        public AssignedAssessorController(IAssignedAssessorService assignedAssessorService, ILogger<AssignedAssessorController> logger)
        {
            _assignedAssessorService = assignedAssessorService;
            _logger = logger;
        }
        public async Task<IActionResult> Index(
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

                var request = new AssignedAssessorRequest
                {                    
                    SortBy = sortBy,
                    SortDirection = sortOrder,
                    Page = page,
                    PageSize = pageSize
                };

                // Get audit logs with sorting and pagination
                var result = await _assignedAssessorService.GetAssignedAssessor(request);

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
                _logger.LogError(ex, "Error retrieving assigned assessor(s)");
                TempData["ErrorMessage"] = "An error occurred while retrieving the notification history.";

                // Return empty result
                var emptyResult = new AssignedAssessorResponse
                {
                    Items = new List<AssignedAssessor>(),
                    TotalCount = 0,
                    TotalPages = 0
                };

                ViewBag.CurrentSort = sortBy;
                ViewBag.CurrentOrder = sortOrder ?? "asc";
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 0;
                ViewBag.TotalItems = 0;                

                return View(emptyResult);
            }
        }
    }
}
