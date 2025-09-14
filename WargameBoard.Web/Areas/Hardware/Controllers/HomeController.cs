using Microsoft.AspNetCore.Mvc;

namespace WargameBoard.Web.Areas.Hardware.Controllers
{
    [Area("Hardware")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
