using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class AuditHistoryPOCController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditHistoryPOCController> _logger;

        public AuditHistoryPOCController(IAuditService auditService, ILogger<AuditHistoryPOCController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string hnId)
        {
            this.ShowBackButton("HeatNetworks", "UserManagement");
            if (string.IsNullOrWhiteSpace(hnId))
            {
                return BadRequest("Heat Network ID is required.");
            }

            try
            {
                // Fetch the data using the refactored method
                var auditLogs = await _auditService.GetAuditHistoryByHnId(hnId.ToUpper());

                // Pass the list to the View
                return View(auditLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit history for HN ID: {HnId}", hnId);

                // In a real GOV.UK app, you'd redirect to a standard "Problem with the service" page
                return View("Error");
            }
        }
    }
}
