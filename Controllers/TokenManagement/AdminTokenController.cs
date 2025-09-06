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
                        TokenCount = g.Tokens.Count
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
        public async Task<ActionResult<List<TeamAssignmentInfo>>> GetGroupAssignments(Guid groupId)
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
        public async Task<ActionResult<AdminAssignmentResult>> RemoveAssignment(Guid assignmentId)
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


        /// <summary>
        /// Create a manual token (without physical characteristics)
        /// </summary>
        [HttpPost("create-manual-token")]
        public async Task<ActionResult<ManualTokenResult>> CreateManualToken([FromBody] CreateManualTokenRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                // Get user details to determine team
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUser.ApplicationUserId);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                // Find the team by TeamCode and SubTeamCode
                var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamCode == user.TeamCode && t.SubTeamCode == user.SubTeamCode);
                if (team == null)
                {
                    return Unauthorized("Team not found");
                }

                var teamId = team.Id;

                // Generate unique token ID
                var tokenId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var token = new Token
                {
                    Id = tokenId,
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    TeamId = teamId,
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName,
                    TokenGroupId = request.TokenGroupId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    TrainingConsistency = 0 // Manual tokens have no training consistency
                    // Note: No TokenSignature - this makes it a "manual" token
                };

                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created manual token: {TokenName} by user {UserId}", 
                    request.Name, currentUser.ApplicationUserId);

                return Ok(new ManualTokenResult
                {
                    Success = true,
                    Message = "Manual token created successfully",
                    Token = new ManualTokenInfo
                    {
                        Name = token.Name,
                        Description = token.Description,
                        Category = token.Category,
                        TokenGroupId = token.TokenGroupId,
                        CreatedAt = token.CreatedAt,
                        CreatedByUserName = token.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual token");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all teams for assignment dropdown
        /// </summary>
        [HttpGet("teams")]
        public async Task<ActionResult<List<TeamInfo>>> GetTeams()
        {
            try
            {
                var teams = await _context.Teams
                    .Where(t => t.IsActive)
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
                        MemberCount = t.Users.Count
                    })
                    .OrderBy(t => t.Name)
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
        /// Update token group information
        /// </summary>
        [HttpPut("update-group/{groupId}")]
        public async Task<ActionResult<AdminTokenGroupResult>> UpdateTokenGroup(Guid groupId, [FromBody] UpdateTokenGroupRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var group = await _context.TokenGroups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group == null)
                {
                    return NotFound("Token group not found");
                }

                // Check if group code already exists (excluding current group)
                var existingGroup = await _context.TokenGroups
                    .FirstOrDefaultAsync(g => g.GroupCode == request.GroupCode && g.Id != groupId);

                if (existingGroup != null)
                {
                    return BadRequest($"Token group with code '{request.GroupCode}' already exists");
                }

                // Update group properties
                group.Name = request.Name;
                group.Description = request.Description;
                group.GroupCode = request.GroupCode;
                group.Category = request.Category;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated token group: {GroupName} ({GroupCode}) by user {UserId}", 
                    request.Name, request.GroupCode, currentUser.ApplicationUserId);

                return Ok(new AdminTokenGroupResult
                {
                    Success = true,
                    Message = "Token group updated successfully",
                    TokenGroup = new TokenGroupInfo
                    {
                        Id = group.Id,
                        Name = group.Name,
                        Description = group.Description,
                        GroupCode = group.GroupCode,
                        Category = group.Category,
                        IsActive = group.IsActive,
                        TokenCount = group.Tokens.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token group");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete token group
        /// </summary>
        [HttpDelete("delete-group/{groupId}")]
        public async Task<ActionResult<AdminTokenGroupResult>> DeleteTokenGroup(Guid groupId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var group = await _context.TokenGroups
                    .Include(g => g.Tokens)
                    .FirstOrDefaultAsync(g => g.Id == groupId);

                if (group == null)
                {
                    return NotFound("Token group not found");
                }

                // Check if group has active tokens
                var activeTokens = group.Tokens.Count(t => t.IsActive);
                if (activeTokens > 0)
                {
                    return BadRequest($"Cannot delete token group with {activeTokens} active tokens. Please remove or reassign tokens first.");
                }

                // Check if group has active team assignments
                var activeAssignments = await _context.TeamTokenGroupAssignments
                    .CountAsync(a => a.TokenGroupId == groupId && a.IsActive);

                if (activeAssignments > 0)
                {
                    return BadRequest($"Cannot delete token group with {activeAssignments} active team assignments. Please remove assignments first.");
                }

                // Soft delete the group
                group.IsActive = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted token group: {GroupName} ({GroupCode}) by user {UserId}", 
                    group.Name, group.GroupCode, currentUser.ApplicationUserId);

                return Ok(new AdminTokenGroupResult
                {
                    Success = true,
                    Message = "Token group deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting token group");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all team assignments for a specific team
        /// </summary>
        [HttpGet("team-assignments/{teamId}")]
        public async Task<ActionResult<List<TokenGroupAssignmentInfo>>> GetTeamAssignments(Guid teamId)
        {
            try
            {
                var assignments = await _context.TeamTokenGroupAssignments
                    .Where(a => a.TeamId == teamId && a.IsActive)
                    .Include(a => a.TokenGroup)
                    .Select(a => new TokenGroupAssignmentInfo
                    {
                        Id = a.Id,
                        TokenGroupId = a.TokenGroupId,
                        TokenGroupName = a.TokenGroup.Name,
                        TokenGroupCode = a.TokenGroup.GroupCode,
                        AssignedAt = a.AssignedAt,
                        AssignedByUserName = a.AssignedByUserName
                    })
                    .ToListAsync();

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team assignments");
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
        public Guid TeamId { get; set; }
        public Guid TokenGroupId { get; set; }
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
        public Guid Id { get; set; }
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
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? AssignedByUserName { get; set; }
    }

    /// <summary>
    /// Request to create a manual token
    /// </summary>
    public class CreateManualTokenRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public Guid? TokenGroupId { get; set; }
    }

    /// <summary>
    /// Result of manual token creation
    /// </summary>
    public class ManualTokenResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ManualTokenInfo? Token { get; set; }
    }

    /// <summary>
    /// Manual token information
    /// </summary>
    public class ManualTokenInfo : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public Guid? TokenGroupId { get; set; }
        public string? CreatedByUserName { get; set; }
    }

    /// <summary>
    /// Team information for dropdowns
    /// </summary>

    /// <summary>
    /// Request to update a token group
    /// </summary>
    public class UpdateTokenGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    /// <summary>
    /// Token group assignment information
    /// </summary>
    public class TokenGroupAssignmentInfo
    {
        public Guid Id { get; set; }
        public Guid TokenGroupId { get; set; }
        public string TokenGroupName { get; set; } = string.Empty;
        public string TokenGroupCode { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string? AssignedByUserName { get; set; }
    }

    /// <summary>
    /// Team information
    /// </summary>
    public class TeamInfo:BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public string? SubTeamCode { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
    }
}
