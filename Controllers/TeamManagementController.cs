using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.DTOs;
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

        public TeamManagementController( ApplicationDbContext context,IUserSessionService userSessionService,ILogger<TeamManagementController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var teamTypes = await _context.TeamTypes.AsNoTracking().Select(tt => new { tt.Id, tt.Name }).ToListAsync();
            ViewData["TeamTypeId"] = new SelectList(teamTypes, "Id", "Name");
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Members(int id)
        {
            return View();
        }

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
                    CreatedBy = currentUser.FullName,
                    Category = request.TeamCategory,
                    ForceType = request.ForceType
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created team: {TeamName} ({TeamCode}) by user {UserId}", 
                    request.Name, request.TeamCode, currentUser.ApplicationUserId);

                return RedirectToAction("Index", "TeamManagement");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTeam([FromBody] CreateTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Find existing team by Id
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == (Guid)request.id);
                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found" });
                }

                

                // Update existing team
                team.Name = request.Name;
                team.TeamCode = request.TeamCode;
                team.SubTeamCode = request.SubTeamCode;
                team.Description = request.Description;
                team.Category = request.TeamCategory;
                team.UpdatedDate = DateTime.UtcNow;
                team.UpdatedBy = currentUser.ApplicationUserId;

                _context.Teams.Update(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated team: {TeamName} ({TeamCode}) by user {UserId}",
                    request.Name, request.TeamCode, currentUser.ApplicationUserId);

                return Json(new { success = true, message = "Team updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team");
                return Json(new { success = false, message = "Internal server error" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found" });
                }

                // Hard delete (permanent)
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Hard deleted team: {TeamName} ({TeamCode}) by user {UserId}",
                    team.Name, team.TeamCode, currentUser.ApplicationUserId);

                return Json(new { success = true, message = "Team deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team");
                return Json(new { success = false, message = "Internal server error" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetTeam(Guid id)
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
                        createdAt = t.CreatedDate ?? DateTime.Now,
                        createdByUserName = t.CreatedBy,
                        category = t.Category,
                    })
                    .FirstOrDefaultAsync(c=>c.id==id);

                return Json(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet]
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
                        createdAt = t.CreatedDate ?? DateTime.Now,
                        createdByUserName = t.CreatedBy,
                        category=t.Category
                      
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


        
        [HttpGet]
        public async Task<IActionResult> GetTeamMembers(Guid? teamId)
        {
            try
            {
                var members = await _context.Users.Include(x=>x.Team)                    .Where(u => u.TeamId == teamId)
                    .Select(u => new
                    {
                        id = u.Id,
                        userName = u.UserName,
                        fullName = u.FullName,
                        email = u.Email,
                        teamId = u.TeamId,
                        teamCode = u.Team.TeamCode,
                        subTeamCode = u.Team.SubTeamCode,
                        assignedDate=u.AssignDate,
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
                user.AssignDate = DateTime.UtcNow;
                
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("Concurrency conflict when assigning user {UserId} to team {TeamId}", 
                        request.UserId, request.TeamId);
                    return Json(new { success = false, message = "User data has been modified by another user. Please refresh and try again." });
                }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromTeam(string userId, Guid teamId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.TeamId == teamId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User is not a member of this team" });
                }

                // Unassign user from team
                user.TeamId = null;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("Concurrency conflict when removing user {UserId} from team {TeamId}", 
                        userId, teamId);
                    return Json(new { success = false, message = "User data has been modified by another user. Please refresh and try again." });
                }

                _logger.LogInformation("Removed user {UserId} from team {TeamId} by {AdminUserId}",
                    userId, teamId, currentUser.ApplicationUserId);

                return Json(new { success = true, message = "User removed from team successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from team {TeamId}", userId, teamId);
                return Json(new { success = false, message = "Internal server error" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.IsActive)
                    .Select(u => new
                    {
                        id = u.Id,
                        userName = u.UserName,
                        fullName = u.FullName,
                        email = u.Email,
                        teamId = u.TeamId,
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
}
