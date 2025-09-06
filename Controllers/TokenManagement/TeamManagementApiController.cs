using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers.TokenManagement
{
    /// <summary>
    /// Team Management API Controller
    /// Handles team CRUD operations and user assignments
    /// </summary>
    [AuthorizeDynamic]
    [ApiController]
    [Route("api/admin/[controller]")]
    public class TeamManagementApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<TeamManagementApiController> _logger;

        public TeamManagementApiController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<TeamManagementApiController> logger)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new team
        /// </summary>
        [HttpPost("create-team")]
        public async Task<ActionResult<TeamResult>> CreateTeam([FromBody] CreateTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                // Check if team code already exists
                var existingTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TeamCode == request.TeamCode);

                if (existingTeam != null)
                {
                    return BadRequest($"Team with code '{request.TeamCode}' already exists");
                }

                var team = new Team
                {
                    Name = request.Name,
                    TeamCode = request.TeamCode,
                    SubTeamCode = request.SubTeamCode,
                    Description = request.Description,
                    Category = request.Category,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created team: {TeamName} ({TeamCode}) by user {UserId}", 
                    request.Name, request.TeamCode, currentUser.ApplicationUserId);

                return Ok(new TeamResult
                {
                    Success = true,
                    Message = "Team created successfully",
                    Team = new TeamInfo
                    {
                        Id = team.Id,
                        Name = team.Name,
                        TeamCode = team.TeamCode,
                        SubTeamCode = team.SubTeamCode,
                        Description = team.Description,
                        Category = team.Category,
                        IsActive = team.IsActive,
                        CreatedAt = team.CreatedAt,
                        MemberCount = 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all teams
        /// </summary>
        [HttpGet("teams")]
        public async Task<ActionResult<List<TeamInfo>>> GetTeams()
        {
            try
            {
                var teams = await _context.Teams
                    .OrderBy(t => t.Name)
                    .Select(t => new TeamInfo
                    {
                        Id = t.Id,
                        Name = t.Name,
                        TeamCode = t.TeamCode,
                        SubTeamCode = t.SubTeamCode,
                        Description = t.Description,
                        Category = t.Category,
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt,
                        MemberCount = t.Users.Count(u => u.IsActive && !u.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teams");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get team members
        /// </summary>
        [HttpGet("team-members/{teamId}")]
        public async Task<ActionResult<List<UserInfo>>> GetTeamMembers(Guid teamId)
        {
            try
            {
                var members = await _context.Users
                    .Where(u => u.TeamId == teamId && u.IsActive && !u.IsDeleted)
                    .Select(u => new UserInfo
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = u.FullName,
                        Email = u.Email,
                        Designation = u.Designation,
                        Department = u.Department,
                        TeamCode = u.TeamCode,
                        SubTeamCode = u.SubTeamCode,
                        IsActive = u.IsActive,
                        CreatedDate = u.CreatedDate
                    })
                    .ToListAsync();

                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team members");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Assign user to team
        /// </summary>
        [HttpPost("assign-user-to-team")]
        public async Task<ActionResult<AssignmentResult>> AssignUserToTeam([FromBody] AssignUserToTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.TeamId);
                if (team == null)
                {
                    return NotFound("Team not found");
                }

                // Update user's team assignment
                user.TeamId = request.TeamId;
                user.TeamCode = team.TeamCode;
                user.SubTeamCode = team.SubTeamCode;
                user.ModifiedBy = currentUser.FullName;
                user.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Assigned user {UserId} to team {TeamId} by user {CurrentUserId}", 
                    request.UserId, request.TeamId, currentUser.ApplicationUserId);

                return Ok(new AssignmentResult
                {
                    Success = true,
                    Message = "User assigned to team successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user to team");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Remove user from team
        /// </summary>
        [HttpPost("remove-user-from-team")]
        public async Task<ActionResult<AssignmentResult>> RemoveUserFromTeam([FromBody] RemoveUserFromTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Remove user from team
                user.TeamId = null;
                user.TeamCode = string.Empty;
                user.SubTeamCode = string.Empty;
                user.ModifiedBy = currentUser.FullName;
                user.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed user {UserId} from team by user {CurrentUserId}", 
                    request.UserId, currentUser.ApplicationUserId);

                return Ok(new AssignmentResult
                {
                    Success = true,
                    Message = "User removed from team successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from team");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all users for assignment dropdown
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<List<UserInfo>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .Select(u => new UserInfo
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = u.FullName,
                        Email = u.Email,
                        Designation = u.Designation,
                        Department = u.Department,
                        TeamCode = u.TeamCode,
                        SubTeamCode = u.SubTeamCode,
                        TeamId = u.TeamId,
                        IsActive = u.IsActive,
                        CreatedDate = u.CreatedDate
                    })
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update team information
        /// </summary>
        [HttpPut("update-team/{teamId}")]
        public async Task<ActionResult<TeamResult>> UpdateTeam(Guid teamId, [FromBody] UpdateTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
                if (team == null)
                {
                    return NotFound("Team not found");
                }

                // Check if team code already exists (excluding current team)
                var existingTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TeamCode == request.TeamCode && t.Id != teamId);

                if (existingTeam != null)
                {
                    return BadRequest($"Team with code '{request.TeamCode}' already exists");
                }

                // Update team properties
                team.Name = request.Name;
                team.TeamCode = request.TeamCode;
                team.SubTeamCode = request.SubTeamCode;
                team.Description = request.Description;
                team.Category = request.Category;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated team: {TeamName} ({TeamCode}) by user {UserId}", 
                    request.Name, request.TeamCode, currentUser.ApplicationUserId);

                return Ok(new TeamResult
                {
                    Success = true,
                    Message = "Team updated successfully",
                    Team = new TeamInfo
                    {
                        Id = team.Id,
                        Name = team.Name,
                        TeamCode = team.TeamCode,
                        SubTeamCode = team.SubTeamCode,
                        Description = team.Description,
                        Category = team.Category,
                        IsActive = team.IsActive,
                        MemberCount = team.Users.Count(u => u.IsActive && !u.IsDeleted)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete team
        /// </summary>
        [HttpDelete("delete-team/{teamId}")]
        public async Task<ActionResult<TeamResult>> DeleteTeam(Guid teamId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var team = await _context.Teams
                    .Include(t => t.Users)
                    .FirstOrDefaultAsync(t => t.Id == teamId);

                if (team == null)
                {
                    return NotFound("Team not found");
                }

                // Check if team has active users
                var activeUsers = team.Users.Count(u => u.IsActive && !u.IsDeleted);
                if (activeUsers > 0)
                {
                    return BadRequest($"Cannot delete team with {activeUsers} active members. Please reassign or remove members first.");
                }

                // Soft delete the team
                team.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted team: {TeamName} ({TeamCode}) by user {UserId}", 
                    team.Name, team.TeamCode, currentUser.ApplicationUserId);

                return Ok(new TeamResult
                {
                    Success = true,
                    Message = "Team deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Remove user from team by user ID
        /// </summary>
        [HttpDelete("remove-user-from-team/{userId}")]
        public async Task<ActionResult<AssignmentResult>> RemoveUserFromTeamById(string userId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Remove user from team
                user.TeamId = null;
                user.TeamCode = string.Empty;
                user.SubTeamCode = string.Empty;
                user.ModifiedBy = currentUser.FullName;
                user.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed user {UserId} from team by user {CurrentUserId}", 
                    userId, currentUser.ApplicationUserId);

                return Ok(new AssignmentResult
                {
                    Success = true,
                    Message = "User removed from team successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from team");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update user role in team
        /// </summary>
        [HttpPut("update-user-role")]
        public async Task<ActionResult<AssignmentResult>> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Update user role (you might need to add a Role field to ApplicationUser)
                // For now, we'll update the Designation field
                user.Designation = request.Role;
                user.ModifiedBy = currentUser.FullName;
                user.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated user {UserId} role to {Role} by user {CurrentUserId}", 
                    request.UserId, request.Role, currentUser.ApplicationUserId);

                return Ok(new AssignmentResult
                {
                    Success = true,
                    Message = "User role updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Request to create a team
    /// </summary>
    public class CreateTeamRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public string? SubTeamCode { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// Request to assign user to team
    /// </summary>
    public class AssignUserToTeamRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid TeamId { get; set; }
    }

    /// <summary>
    /// Request to remove user from team
    /// </summary>
    public class RemoveUserFromTeamRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of team operations
    /// </summary>
    public class TeamResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TeamInfo? Team { get; set; }
    }

    /// <summary>
    /// Result of assignment operations
    /// </summary>
    public class AssignmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to update a team
    /// </summary>
    public class UpdateTeamRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public string? SubTeamCode { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// Request to update user role
    /// </summary>
    public class UpdateUserRoleRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid TeamId { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// User information
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public string SubTeamCode { get; set; } = string.Empty;
        public Guid? TeamId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Role { get; set; }
        public DateTime? AssignedDate { get; set; }
    }
}
