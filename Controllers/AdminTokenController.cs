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
    /// Handles UI pages for token group management and token creation
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
        /// Display admin token management dashboard
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
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
        /// Display token creation page
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ===== API ENDPOINTS FOR AJAX CALLS =====

        /// <summary>
        /// Create a new token group (e.g., "Company A", "Brigade 1")
        /// </summary>
        [HttpPost("create-group")]
        public async Task<IActionResult> CreateTokenGroup([FromBody] CreateTokenGroupRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Check if group code already exists
                var existingGroup = await _context.TokenGroups
                    .FirstOrDefaultAsync(g => g.GroupCode == request.GroupCode);

                if (existingGroup != null)
                {
                    return Json(new { success = false, message = $"Token group with code '{request.GroupCode}' already exists" });
                }

                var tokenGroup = new TokenGroup
                {
                    Name = request.Name,
                    Description = request.Description,
                    GroupCode = request.GroupCode,
                    Category = request.Category,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
                };

                _context.TokenGroups.Add(tokenGroup);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created token group: {GroupName} ({GroupCode}) by user {UserId}", 
                    request.Name, request.GroupCode, currentUser.ApplicationUserId);

                return Json(new
                {
                    success = true,
                    message = "Token group created successfully",
                    tokenGroup = new
                    {
                        id = tokenGroup.Id,
                        name = tokenGroup.Name,
                        description = tokenGroup.Description,
                        groupCode = tokenGroup.GroupCode,
                        category = tokenGroup.Category,
                        isActive = tokenGroup.IsActive,
                        createdAt = tokenGroup.CreatedAt,
                        createdByUserName = tokenGroup.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token group");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all token groups for admin management
        /// </summary>
        [HttpGet("groups")]
        public async Task<IActionResult> GetTokenGroups()
        {
            try
            {
                var groups = await _context.TokenGroups
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.Name)
                    .Select(g => new
                    {
                        id = g.Id,
                        name = g.Name,
                        description = g.Description,
                        groupCode = g.GroupCode,
                        category = g.Category,
                        isActive = g.IsActive,
                        createdAt = g.CreatedAt,
                        createdByUserName = g.CreatedByUserName
                    })
                    .ToListAsync();

                return Json(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token groups");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a token (without physical characteristics)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateToken([FromBody] CreateTokenRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get user details to determine team
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUser.ApplicationUserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Find the team by TeamCode and SubTeamCode
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamCode == user.TeamCode && t.SubTeamCode == user.SubTeamCode);
                if (team == null)
                {
                    return Json(new { success = false, message = "User team not found" });
                }

                // Create the token
                var token = new Token
                {
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    TokenGroupId = request.TokenGroupId,
                    TeamId = team.Id,
                    IsManualToken = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
                };

                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created token: {TokenName} by user {UserId}", 
                    request.Name, currentUser.ApplicationUserId);

                return Json(new
                {
                    success = true,
                    message = "Token created successfully",
                    token = new
                    {
                        id = token.Id,
                        name = token.Name,
                        description = token.Description,
                        category = token.Category,
                        tokenGroupId = token.TokenGroupId,
                        teamId = token.TeamId,
                        isManualToken = token.IsManualToken,
                        isActive = token.IsActive,
                        createdAt = token.CreatedAt,
                        createdByUserName = token.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all teams for admin management
        /// </summary>
        [HttpGet("teams")]
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                var teams = await _context.Teams
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        teamCode = t.TeamCode,
                        subTeamCode = t.SubTeamCode,
                        description = t.Description,
                        isActive = t.IsActive,
                        createdAt = t.CreatedAt
                    })
                    .ToListAsync();

                return Json(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teams");
                return Json(new { success = false, message = "Internal server error" });
            }
        }
    }

    // ===== REQUEST/RESPONSE MODELS =====

    public class CreateTokenGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class CreateTokenRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid? TokenGroupId { get; set; }
    }
}
