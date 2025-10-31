using Microsoft.AspNetCore.Mvc;

namespace TechWebSol.Controllers
{
    public class WarGameRepositoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
