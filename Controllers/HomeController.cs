using TechWebSol.Filters;
using TechWebSol.ViewModels;
using TechWebSol.Services;
using TechWebSol.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace TechWebSol.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ApplicationUserVM applicatonUser;
        
        public HomeController(ApplicationDbContext context
            , IWebHostEnvironment hostEnvironment
            , IUserSessionService IUserSessionService
            )
        {
            _context = context;
            webHostEnvironment = hostEnvironment;
            applicatonUser = IUserSessionService.GetCurrentUser();
        }

        public IActionResult Index()
        {
            // Double-check that user session is valid
            if (applicatonUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            return View();
        }

        [DisplayName("Configuration: Dropdown Permissions")]
        public IActionResult ListOfValues()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}


