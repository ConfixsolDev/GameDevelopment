using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWebSol.Filters;

namespace TechWebSol.Controllers.TokenManagement
{
    [Authorize]

    public class TokenSystemController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Token Training & Identification System";
            ViewData["Subtitle"] = "Train, test, and manage touch-based authentication tokens";
            return View();
        }
    }
}
