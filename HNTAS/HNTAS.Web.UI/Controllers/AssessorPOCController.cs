using HNTAS.Api.Client.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class AssessorPOCController : Controller
    {
        private readonly ILogger<AssessorPOCController> _logger;
        private readonly IAssessorApi _assessorApi;

        public AssessorPOCController(ILogger<AssessorPOCController> logger, IAssessorApi assessorApi)
        {
            _logger = logger;
            _assessorApi = assessorApi;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<JsonResult> Search(string q)
        {
            try
            {
                if (string.IsNullOrEmpty(q) || q.Length < 2)
                {
                    return Json(new List<string>());
                }

                var results = await _assessorApi.ApiAssessorSearchGetAsync(q);
                if (results.IsOk)
                {
                    return Json(results.Ok());
                }
                else
                {
                    _logger.LogWarning("Assessor search API returned non-OK status");
                    return Json(new { error = "Failed to fetch assessor suggestions" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching assessor suggestions");
                return Json(new { error = "Internal server error" });
            }
        }
    }
}
