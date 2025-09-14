using Microsoft.AspNetCore.Mvc;

namespace WargameBoard.Web.Areas.Tokens.Controllers
{
    [Area("Tokens")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
