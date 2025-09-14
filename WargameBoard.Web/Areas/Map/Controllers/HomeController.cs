using Microsoft.AspNetCore.Mvc;

namespace WargameBoard.Web.Areas.Map.Controllers
{
    [Area("Map")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
