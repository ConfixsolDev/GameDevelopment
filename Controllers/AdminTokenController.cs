using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    /// <summary>
    /// Admin Token Management MVC Controller
    /// Handles UI pages for token group management and manual token creation
    /// </summary>
    [AuthorizeDynamic]
    public class AdminTokenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<AdminTokenController> _logger;

        public AdminTokenController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<AdminTokenController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        /// <summary>
        /// Display token group management page
        /// </summary>
        [HttpGet]
        public IActionResult ManageTokenGroups()
        {
            return View();
        }

        /// <summary>
        /// Display manual token creation page
        /// </summary>
        [HttpGet]
        public IActionResult CreateManualToken()
        {
            return View();
        }
    }
}
