using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWebSol.Filters;

namespace TechWebSol.Controllers.TokenManagement
{
    [AuthorizeDynamic]
    public class TokenManagmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
