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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // ===== API ENDPOINTS FOR AJAX CALLS =====

        /// <summary>
        /// Start a new game session
        /// </summary>
        public async Task<IActionResult> StartGameSession([FromBody] StartGameSessionRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Check if session code already exists
                var existingSession = await _context.GameSessions
                    .FirstOrDefaultAsync(s => s.SessionCode == request.SessionCode);

                if (existingSession != null)
                {
                    return Json(new { success = false, message = $"Game session with code '{request.SessionCode}' already exists" });
                }

                var gameSession = new GameSession
                {
                    Name = request.Name,
                    SessionCode = request.SessionCode,
                    Description = request.Description,
                    Status = "Active",
                };

                _context.GameSessions.Add(gameSession);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created game session: {SessionName} ({SessionCode}) by user {UserId}", 
                    request.Name, request.SessionCode, currentUser.ApplicationUserId);

                return Json(new
                {
                    success = true,
                    message = "Game session started successfully",
                    session = new
                    {
                        id = gameSession.Id,
                        name = gameSession.Name,
                        sessionCode = gameSession.SessionCode,
                        description = gameSession.Description,
                        status = gameSession.Status,
                        createdAt = gameSession.CreatedDate ?? DateTime.Now,
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game session");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// End a game session
        /// </summary>
        public async Task<IActionResult> EndGameSession(int sessionId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var session = await _context.GameSessions.FindAsync(sessionId);
                if (session == null)
                {
                    return Json(new { success = false, message = "Game session not found" });
                }

                session.Status = "Comlpeted";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Ended game session: {SessionName} ({SessionCode}) by user {UserId}", 
                    session.Name, session.SessionCode, currentUser.ApplicationUserId);

                return Json(new
                {
                    success = true,
                    message = "Game session ended successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending game session");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get active game sessions
        /// </summary>
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                var sessions = await _context.GameSessions
                    .Where(s => s.Status =="Active")
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        sessionCode = s.SessionCode,
                        description = s.Description,
                        Status = "Active",
                        createdAt = s.CreatedDate ?? DateTime.Now,
                    })
                    .ToListAsync();

                return Json(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get free tokens
        /// </summary>
        [HttpGet("free-tokens")]
        public async Task<IActionResult> GetFreeTokens()
        {
            try
            {
                var tokens = await _context.Tokens
                    .Where(t => t.IsActive && !t.IsManualToken)
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        description = t.Description,
                        category = t.Category,
                        isActive = t.IsActive,
                        createdAt = t.CreatedDate ?? DateTime.Now
                    })
                    .ToListAsync();

                return Json(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting free tokens");
                return Json(new { success = false, message = "Internal server error" });
            }
        }
    }

    // ===== REQUEST/RESPONSE MODELS =====

    public class StartGameSessionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SessionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
