using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    /// <summary>
    /// Team Management Controller
    /// Handles team creation, management, and user assignment
    /// </summary>
    [AuthorizeDynamic]
    public class TeamManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<TeamManagementController> _logger;

        public TeamManagementController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<TeamManagementController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        /// <summary>
        /// Display team management page
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Display team creation page
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Display team edit page
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View();
        }

        /// <summary>
        /// Display team members page
        /// </summary>
        [HttpGet]
        public IActionResult Members(int id)
        {
            return View();
        }

        // ===== API ENDPOINTS FOR AJAX CALLS =====

        /// <summary>
        /// Create a new team
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Check if team code already exists
                var existingTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TeamCode == request.TeamCode);

                if (existingTeam != null)
                {
                    return Json(new { success = false, message = $"Team with code '{request.TeamCode}' already exists" });
                }

                var team = new Team
                {
                    Name = request.Name,
                    TeamCode = request.TeamCode,
                    SubTeamCode = request.SubTeamCode,
                    Description = request.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created team: {TeamName} ({TeamCode}) by user {UserId}", 
                    request.Name, request.TeamCode, currentUser.ApplicationUserId);

                return Json(new
                {
                    success = true,
                    message = "Team created successfully",
                    team = new
                    {
                        id = team.Id,
                        name = team.Name,
                        teamCode = team.TeamCode,
                        subTeamCode = team.SubTeamCode,
                        description = team.Description,
                        isActive = team.IsActive,
                        createdAt = team.CreatedAt,
                        createdByUserName = team.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all teams
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
                        createdAt = t.CreatedAt,
                        createdByUserName = t.CreatedByUserName
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

        /// <summary>
        /// Get team members
        /// </summary>
        [HttpGet("team-members/{teamId}")]
        public async Task<IActionResult> GetTeamMembers(Guid? teamId)
        {
            try
            {
                var members = await _context.Users
                    .Where(u => u.TeamId == teamId)
                    .Select(u => new
                    {
                        id = u.Id,
                        userName = u.UserName,
                        fullName = u.FullName,
                        email = u.Email,
                        teamId = u.TeamId,
                        teamCode = u.TeamCode,
                        subTeamCode = u.SubTeamCode
                    })
                    .ToListAsync();

                return Json(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team members");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Assign user to team
        /// </summary>
        [HttpPost("assign-user-to-team")]
        public async Task<IActionResult> AssignUserToTeam([FromBody] AssignUserToTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var team = await _context.Teams.FindAsync(request.TeamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found" });
                }

                user.TeamId = request.TeamId;
                user.TeamCode = team.TeamCode;
                user.SubTeamCode = team.SubTeamCode;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Assigned user {UserId} to team {TeamId} by user {CurrentUserId}", 
                    request.UserId, request.TeamId, currentUser.ApplicationUserId);

                return Json(new
                {
                    success = true,
                    message = "User assigned to team successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user to team");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FullName)
                    .Select(u => new
                    {
                        id = u.Id,
                        userName = u.UserName,
                        fullName = u.FullName,
                        email = u.Email,
                        teamId = u.TeamId,
                        teamCode = u.TeamCode,
                        subTeamCode = u.SubTeamCode,
                        isActive = u.IsActive
                    })
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return Json(new { success = false, message = "Internal server error" });
            }
        }
    }

    // ===== REQUEST/RESPONSE MODELS =====

    public class CreateTeamRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public string SubTeamCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AssignUserToTeamRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid? TeamId { get; set; }
    }
}
