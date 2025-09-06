using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    /// <summary>
    /// Game Management MVC Controller
    /// Handles UI pages for game session management
    /// </summary>
    [AuthorizeDynamic]
    public class GameManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<GameManagementController> _logger;

        public GameManagementController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<GameManagementController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        /// <summary>
        /// Display game session management page
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Display active game sessions
        /// </summary>
        [HttpGet]
        public IActionResult ActiveSessions()
        {
            return View();
        }

        /// <summary>
        /// Display free tokens management
        /// </summary>
        [HttpGet]
        public IActionResult FreeTokens()
        {
            return View();
        }
    }
}
