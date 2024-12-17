using ClassLibrary;
using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {
        private readonly ICountriesService _countriesService;
        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }
        [Route("[action]")]
        public IActionResult UploadFromExcel()
        {
            
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Please supply an xlsx file";
                return View();
            }
            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Unsupported file type. Please supply an xlsx file.";
                return View();
            }
            int countriesCountInserted = await _countriesService.UploadCountriesFromExcelFile(excelFile);
            ViewBag.Message = $"{countriesCountInserted} countries inserted";
            return View();
        }
    }
}
