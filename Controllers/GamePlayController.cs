using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWebSol.Filters;

namespace TechWebSol.Controllers
{
    [AuthorizeDynamic]
    public class GamePlayController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Game Play Arena";
            ViewData["Subtitle"] = "Strategic Command Center - Fox Land vs Blue Land";
            return View();
        }
    }
}
