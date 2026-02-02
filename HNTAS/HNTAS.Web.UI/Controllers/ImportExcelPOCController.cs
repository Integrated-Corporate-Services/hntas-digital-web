using ClosedXML.Excel;
using HNTAS.Web.UI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class ImportExcelPOCController : Controller
    {
        public IActionResult Index()
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            return View();
        }

        // POST: /ImportExcel/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IFormFile excelFile)
        {
            this.ShowBackButton("UserAccount", "Dashboard");

            var tableData = new List<List<string>>();

            // 1. Basic Validation
            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("excelFile", "Select an Excel file to upload");
                return View("Index", tableData);
            }

            var fileExtension = Path.GetExtension(excelFile.FileName).ToLower();
            if (fileExtension != ".xlsx")
            {
                ModelState.AddModelError("excelFile", "The selected file must be an XLSX");
                return View("Index", tableData);
            }

            // Checking ContentType (MIME type)
            if (excelFile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                ModelState.AddModelError("excelFile", "The selected file must be a real Excel spreadsheet");
                return View("Index", tableData);
            }

            try
            {
                // 2. Processing the Stream
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);

                        // Use RangeUsed to ensure we don't process millions of empty rows
                        var range = worksheet.RangeUsed();
                        if (range != null)
                        {
                            var rows = range.RowsUsed();
                            foreach (var row in rows)
                            {
                                var rowData = row.Cells(1, range.ColumnCount())
                                                 .Select(c => c.Value.ToString())
                                                 .ToList();
                                tableData.Add(rowData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "There was a problem reading the file. Ensure it is not password protected.");
                return View("Index", tableData);
            }

            // 3. Return to the same Index view with the data
            return View("Index", tableData);
        }
    }
}
