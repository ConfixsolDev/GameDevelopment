using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers.TokenManagement
{
    /// <summary>
    /// Admin Token Management Controller
    /// Handles token group creation and team assignments by administrators
    /// </summary>
    [AuthorizeDynamic]
    [ApiController]
    [Route("api/admin/[controller]")]
    public class AdminTokenController : ControllerBase
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
        /// Create a new token group (e.g., "Company A", "Brigade 1")
        /// </summary>
        [HttpPost("create-group")]
        public async Task<ActionResult<AdminTokenGroupResult>> CreateTokenGroup([FromBody] CreateTokenGroupRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                // Check if group code already exists
                var existingGroup = await _context.TokenGroups
                    .FirstOrDefaultAsync(g => g.GroupCode == request.GroupCode);

                if (existingGroup != null)
                {
                    return BadRequest($"Token group with code '{request.GroupCode}' already exists");
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

                return Ok(new AdminTokenGroupResult
                {
                    Success = true,
                    Message = "Token group created successfully",
                    TokenGroup = new TokenGroupInfo
                    {
                        Id = tokenGroup.Id,
                        Name = tokenGroup.Name,
                        Description = tokenGroup.Description,
                        GroupCode = tokenGroup.GroupCode,
                        Category = tokenGroup.Category,
                        IsActive = tokenGroup.IsActive,
                        CreatedAt = tokenGroup.CreatedAt,
                        CreatedByUserName = tokenGroup.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token group");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all token groups for admin management
        /// </summary>
        [HttpGet("groups")]
        public async Task<ActionResult<List<TokenGroupInfo>>> GetTokenGroups()
        {
            try
            {
                var groups = await _context.TokenGroups
                    .OrderBy(g => g.Name)
                    .Select(g => new TokenGroupInfo
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Description = g.Description,
                        GroupCode = g.GroupCode,
                        Category = g.Category,
                        IsActive = g.IsActive,
                        CreatedAt = g.CreatedAt,
                        CreatedByUserName = g.CreatedByUserName,
                        TokenCount = g.Tokens.Count + g.SimplifiedTokens.Count
                    })
                    .ToListAsync();

                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token groups");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Assign token group to a team
        /// </summary>
        [HttpPost("assign-group-to-team")]
        public async Task<ActionResult<AdminAssignmentResult>> AssignGroupToTeam([FromBody] AssignGroupToTeamRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                // Check if assignment already exists
                var existingAssignment = await _context.TeamTokenGroupAssignments
                    .FirstOrDefaultAsync(a => a.TeamId == request.TeamId && a.TokenGroupId == request.TokenGroupId);

                if (existingAssignment != null)
                {
                    return BadRequest("This token group is already assigned to this team");
                }

                var assignment = new TeamTokenGroupAssignment
                {
                    TeamId = request.TeamId,
                    TokenGroupId = request.TokenGroupId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                    AssignedByUserId = currentUser.ApplicationUserId,
                    AssignedByUserName = currentUser.FullName
                };

                _context.TeamTokenGroupAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assigned token group {GroupId} to team {TeamId} by user {UserId}", 
                    request.TokenGroupId, request.TeamId, currentUser.ApplicationUserId);

                return Ok(new AdminAssignmentResult
                {
                    Success = true,
                    Message = "Token group assigned to team successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning token group to team");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get team assignments for a token group
        /// </summary>
        [HttpGet("group-assignments/{groupId}")]
        public async Task<ActionResult<List<TeamAssignmentInfo>>> GetGroupAssignments(int groupId)
        {
            try
            {
                var assignments = await _context.TeamTokenGroupAssignments
                    .Where(a => a.TokenGroupId == groupId && a.IsActive)
                    .Select(a => new TeamAssignmentInfo
                    {
                        Id = a.Id,
                        TeamId = a.TeamId,
                        AssignedAt = a.AssignedAt,
                        AssignedByUserName = a.AssignedByUserName
                    })
                    .ToListAsync();

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group assignments");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Remove token group assignment from team
        /// </summary>
        [HttpDelete("remove-assignment/{assignmentId}")]
        public async Task<ActionResult<AdminAssignmentResult>> RemoveAssignment(int assignmentId)
        {
            try
            {
                var assignment = await _context.TeamTokenGroupAssignments
                    .FirstOrDefaultAsync(a => a.Id == assignmentId);

                if (assignment == null)
                {
                    return NotFound("Assignment not found");
                }

                assignment.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed token group assignment {AssignmentId}", assignmentId);

                return Ok(new AdminAssignmentResult
                {
                    Success = true,
                    Message = "Assignment removed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing assignment");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Request to create a token group
    /// </summary>
    public class CreateTokenGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    /// <summary>
    /// Request to assign token group to team
    /// </summary>
    public class AssignGroupToTeamRequest
    {
        public string TeamId { get; set; } = string.Empty;
        public int TokenGroupId { get; set; }
    }

    /// <summary>
    /// Result of token group creation
    /// </summary>
    public class AdminTokenGroupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TokenGroupInfo? TokenGroup { get; set; }
    }

    /// <summary>
    /// Result of assignment operations
    /// </summary>
    public class AdminAssignmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token group information
    /// </summary>
    public class TokenGroupInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public int TokenCount { get; set; }
    }

    /// <summary>
    /// Team assignment information
    /// </summary>
    public class TeamAssignmentInfo
    {
        public int Id { get; set; }
        public string TeamId { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string? AssignedByUserName { get; set; }
    }
}
