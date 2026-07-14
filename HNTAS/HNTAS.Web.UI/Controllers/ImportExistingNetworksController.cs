using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class ImportExistingNetworksController : Controller
    {
        private readonly IImportExistingNetworksService _importExistingNetworksService;
        public ImportExistingNetworksController(IImportExistingNetworksService importExistingNetworksService)
        {
            _importExistingNetworksService = importExistingNetworksService;
        }
        public IActionResult Index()
        {
            ViewBag.DisplayResult = false;
            this.ShowBackButton("UserAccount", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IFormFile csvFile)
        {
            ViewBag.DisplayResult = false;
            this.ShowBackButton("UserAccount", "Dashboard");

            // 1. Basic Validation
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError("csvFile", "Select a CSV file to upload");
                return View("Index");
            }

            var fileExtension = Path.GetExtension(csvFile.FileName).ToLower();
            if (fileExtension != ".csv")
            {
                ModelState.AddModelError("csvFile", "The selected file must be a CSV");
                return View("Index");
            }

            try
            {
                using var stream = csvFile.OpenReadStream();
                using var reader = new StreamReader(stream);

                var csv = await reader.ReadToEndAsync();

                var result = await _importExistingNetworksService.ImportCsv(csv);
                ViewBag.DisplayResult = true;
                return View("Index", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "There was a problem reading the file. Ensure it is not password protected.");
                return View("Index");
            }
        }
    }
}
