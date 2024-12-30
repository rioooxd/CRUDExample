using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers
{
    public class ErrorController : Controller
    {
        [Route("[action]")]
        public IActionResult Error()
        {
            IExceptionHandlerPathFeature? details = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.ErrorMessage = "An error occured";
            if(details != null && details.Error != null)
            {
                ViewBag.ErrorMessage = details.Error.Message;
            }
            return View();
        }
    }
}
