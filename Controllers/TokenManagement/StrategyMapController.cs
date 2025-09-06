using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWebSol.Filters;

namespace TechWebSol.Controllers.TokenManagement
{
    [AuthorizeDynamic]
    public class StrategyMapController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
