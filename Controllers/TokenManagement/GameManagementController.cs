using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers.TokenManagement
{
    /// <summary>
    /// Game Management Controller
    /// Handles game sessions, token bindings, and free token management
    /// </summary>
    [AuthorizeDynamic]
    [ApiController]
    [Route("api/game/[controller]")]
    public class GameManagementController : ControllerBase
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
        /// Start a new game session
        /// </summary>
        [HttpPost("start-session")]
        public async Task<ActionResult<GameSessionResult>> StartGameSession([FromBody] StartGameSessionRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                // Check if session code already exists
                var existingSession = await _context.GameSessions
                    .FirstOrDefaultAsync(s => s.SessionCode == request.SessionCode);

                if (existingSession != null)
                {
                    return BadRequest($"Game session with code '{request.SessionCode}' already exists");
                }

                var gameSession = new GameSession
                {
                    Name = request.Name,
                    Description = request.Description,
                    SessionCode = request.SessionCode,
                    StartTime = DateTime.UtcNow,
                    Status = "Active",
                    CreatedByUserId = currentUser.ApplicationUserId,
                    CreatedByUserName = currentUser.FullName
                };

                _context.GameSessions.Add(gameSession);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Started game session: {SessionName} ({SessionCode}) by user {UserId}", 
                    request.Name, request.SessionCode, currentUser.ApplicationUserId);

                return Ok(new GameSessionResult
                {
                    Success = true,
                    Message = "Game session started successfully",
                    GameSession = new GameSessionInfo
                    {
                        Id = gameSession.Id,
                        Name = gameSession.Name,
                        Description = gameSession.Description,
                        SessionCode = gameSession.SessionCode,
                        StartTime = gameSession.StartTime,
                        Status = gameSession.Status,
                        CreatedByUserName = gameSession.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game session");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// End a game session and free all bound tokens
        /// </summary>
        [HttpPost("end-session/{sessionId}")]
        public async Task<ActionResult<GameSessionResult>> EndGameSession(Guid sessionId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var gameSession = await _context.GameSessions
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (gameSession == null)
                {
                    return NotFound("Game session not found");
                }

                if (gameSession.Status != "Active")
                {
                    return BadRequest("Game session is not active");
                }

                // End the game session
                gameSession.EndTime = DateTime.UtcNow;
                gameSession.Status = "Completed";

                // Free all bound tokens by moving them to FreeTokens table
                var boundTokens = await _context.TokenBindings
                    .Where(b => b.GameSessionId == sessionId && b.IsActive)
                    .ToListAsync();

                foreach (var binding in boundTokens)
                {
                    // Move tokens to free tokens table
                    await MoveTokensToFreeTokens(binding);
                    
                    // Mark binding as inactive
                    binding.IsActive = false;
                    binding.UnboundAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Ended game session {SessionId} and freed {TokenCount} tokens", 
                    sessionId, boundTokens.Count);

                return Ok(new GameSessionResult
                {
                    Success = true,
                    Message = $"Game session ended successfully. {boundTokens.Count} tokens have been freed for reuse.",
                    GameSession = new GameSessionInfo
                    {
                        Id = gameSession.Id,
                        Name = gameSession.Name,
                        Description = gameSession.Description,
                        SessionCode = gameSession.SessionCode,
                        StartTime = gameSession.StartTime,
                        EndTime = gameSession.EndTime,
                        Status = gameSession.Status,
                        CreatedByUserName = gameSession.CreatedByUserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending game session");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Bind tokens to entities for the current game session
        /// </summary>
        [HttpPost("bind-tokens")]
        public async Task<ActionResult<BindingResult>> BindTokensToEntities([FromBody] BindTokensRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                if (currentUser == null)
                {
                    return Unauthorized("User not authenticated");
                }

                // Get active game session
                var gameSession = await _context.GameSessions
                    .FirstOrDefaultAsync(s => s.SessionCode == request.SessionCode && s.Status == "Active");

                if (gameSession == null)
                {
                    return BadRequest("Active game session not found");
                }

                var bindings = new List<TokenBinding>();

                foreach (var entity in request.Entities)
                {
                    var binding = new TokenBinding
                    {
                        GameSessionId = gameSession.Id,
                        TokenGroupId = entity.TokenGroupId,
                        TeamId = entity.TeamId,
                        EntityName = entity.EntityName,
                        EntityCode = entity.EntityCode,
                        EntityDescription = entity.EntityDescription,
                        BoundByUserId = currentUser.ApplicationUserId,
                        BoundByUserName = currentUser.FullName
                    };

                    bindings.Add(binding);
                }

                _context.TokenBindings.AddRange(bindings);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bound {BindingCount} token groups to entities in session {SessionCode}", 
                    bindings.Count, request.SessionCode);

                return Ok(new BindingResult
                {
                    Success = true,
                    Message = $"Successfully bound {bindings.Count} token groups to entities"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error binding tokens to entities");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get free tokens available for new game assignment
        /// </summary>
        [HttpGet("free-tokens")]
        public async Task<ActionResult<List<FreeTokenInfo>>> GetFreeTokens()
        {
            try
            {
                var freeTokens = await _context.FreeTokens
                    .OrderBy(t => t.Name)
                    .Select(t => new FreeTokenInfo
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Category = t.Category,
                        TouchCount = t.TouchCount,
                        System = t.System,
                        CreatedAt = t.CreatedAt,
                        LastUsed = t.LastUsed,
                        UsageCount = t.UsageCount,
                        CreatedByUserName = t.CreatedByUserName
                    })
                    .ToListAsync();

                return Ok(freeTokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting free tokens");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get active game sessions
        /// </summary>
        [HttpGet("active-sessions")]
        public async Task<ActionResult<List<GameSessionInfo>>> GetActiveSessions()
        {
            try
            {
                var sessions = await _context.GameSessions
                    .Where(s => s.Status == "Active")
                    .OrderBy(s => s.StartTime)
                    .Select(s => new GameSessionInfo
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        SessionCode = s.SessionCode,
                        StartTime = s.StartTime,
                        Status = s.Status,
                        CreatedByUserName = s.CreatedByUserName,
                        TokenBindingCount = s.TokenBindings.Count(b => b.IsActive)
                    })
                    .ToListAsync();

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Move tokens from bound state to free tokens table
        /// </summary>
        private async Task MoveTokensToFreeTokens(TokenBinding binding)
        {
            // Get all tokens from the bound group
            var allTokens = await _context.Tokens
                .Where(t => t.TokenGroupId == binding.TokenGroupId && t.IsActive)
                .ToListAsync();

            // Move all tokens to free tokens
            foreach (var token in allTokens)
            {
                var freeToken = new FreeToken
                {
                    Id = token.Id,
                    Name = token.Name,
                    Description = token.Description,
                    Category = token.Category,
                    TouchCount = token.Signature?.TouchCount ?? 0,
                    System = "unified", // Now using unified system
                    CreatedAt = token.CreatedAt,
                    LastUsed = token.LastUsed,
                    UsageCount = token.UsageCount,
                    CreatedByUserId = token.CreatedByUserId,
                    CreatedByUserName = token.CreatedByUserName
                };

                _context.FreeTokens.Add(freeToken);
            }
        }
    }

    /// <summary>
    /// Request to start a game session
    /// </summary>
    public class StartGameSessionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SessionCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to bind tokens to entities
    /// </summary>
    public class BindTokensRequest
    {
        public string SessionCode { get; set; } = string.Empty;
        public List<EntityBinding> Entities { get; set; } = new();
    }

    /// <summary>
    /// Entity binding information
    /// </summary>
    public class EntityBinding
    {
        public Guid TokenGroupId { get; set; }
        public Guid TeamId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? EntityCode { get; set; }
        public string? EntityDescription { get; set; }
    }

    /// <summary>
    /// Result of game session operations
    /// </summary>
    public class GameSessionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GameSessionInfo? GameSession { get; set; }
    }

    /// <summary>
    /// Result of binding operations
    /// </summary>
    public class BindingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Game session information
    /// </summary>
    public class GameSessionInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SessionCode { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CreatedByUserName { get; set; }
        public int TokenBindingCount { get; set; }
    }

    /// <summary>
    /// Free token information
    /// </summary>
    public class FreeTokenInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public int TouchCount { get; set; }
        public string System { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public int UsageCount { get; set; }
        public string? CreatedByUserName { get; set; }
    }
}
