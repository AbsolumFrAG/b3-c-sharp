using Microsoft.AspNetCore.Mvc;
using TP_CSharp.Models;

namespace TP_CSharp.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult NotFound()
        {
            var model = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                Message = "Désolé, la page demandée n'est pas disponible.",
            };
            return View(model);
        }
    }
}
