using Microsoft.AspNetCore.Mvc;

namespace WargameBoard.Web.Areas.Units.Controllers
{
    [Area("Units")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
