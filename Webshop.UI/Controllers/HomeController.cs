using Microsoft.AspNetCore.Mvc;

namespace Webshop.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}