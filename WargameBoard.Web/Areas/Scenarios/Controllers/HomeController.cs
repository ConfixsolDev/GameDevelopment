using Microsoft.AspNetCore.Mvc;

namespace WargameBoard.Web.Areas.Scenarios.Controllers
{
    [Area("Scenarios")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
