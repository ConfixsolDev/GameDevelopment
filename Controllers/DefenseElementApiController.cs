using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TechWebSol.DAL;
using TechWebSol.Data;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    /// <summary>
    /// API Controller for Defense Element operations
    /// Handles CRUD operations for defense planning elements (kill zones, minefields, etc.)
    /// Assigns responsibility to tokens
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DefenseElementApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<DefenseElementApiController> _logger;
        private readonly IMemoryCache _memoryCache;

        public DefenseElementApiController(
            ApplicationDbContext context,
            IUserSessionService userSessionService,
            ILogger<DefenseElementApiController> logger,
            IMemoryCache memoryCache)
        {
            _context = context;
            _userSessionService = userSessionService;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Get all defense elements for current game session
        /// </summary>
        [HttpGet("session/{gameSessionId}")]
        public async Task<IActionResult> GetDefenseElementsBySession(Guid gameSessionId)
        {
            try
            {
                var dal = new DefenseElementDAL(_context);
                var elements = await dal.GetDefenseElementsBySessionAsync(gameSessionId);

                return Ok(new
                {
                    success = true,
                    elements = elements,
                    count = elements.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting defense elements for session {GameSessionId}", gameSessionId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get visible defense elements for current user's team
        /// OPTIMIZED: Uses short-term memory cache (10 seconds) to prevent redundant DB calls
        /// </summary>
        [HttpGet("visible/{gameSessionId}")]
        public async Task<IActionResult> GetVisibleDefenseElements(Guid gameSessionId)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                var teamId = currentUser.TeamId ?? Guid.Empty;
                var forceType = currentUser.ForceType ?? "Blueland";

                // Cache key includes gameSessionId, teamId and forceType for proper isolation
                var cacheKey = $"DefenseElements_Visible_{gameSessionId}_{teamId}_{forceType}";
                
                // Try to get from cache first
                if (!_memoryCache.TryGetValue(cacheKey, out List<DefenseElement> elements))
                {
                    _logger.LogInformation("📥 Cache MISS for {CacheKey} - fetching from database", cacheKey);
                    
                    // Cache miss - fetch from database
                    var dal = new DefenseElementDAL(_context);
                    elements = await dal.GetVisibleDefenseElementsAsync(gameSessionId, teamId, forceType);
                    
                    // Store in cache with short expiration (10 seconds)
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(10))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetSize(1)
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _memoryCache.Set(cacheKey, elements, cacheOptions);
                }
                else
                {
                    _logger.LogDebug("⚡ Cache HIT for {CacheKey} - returning cached data", cacheKey);
                }

                return Ok(new
                {
                    success = true,
                    elements = elements,
                    count = elements.Count,
                    teamId = teamId,
                    forceType = forceType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting visible defense elements for session {GameSessionId}", gameSessionId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all visible defense elements for current user's team (no session filter)
        /// OPTIMIZED: Uses short-term memory cache (10 seconds) to prevent redundant DB calls
        /// </summary>
        [HttpGet("team")]
        public async Task<IActionResult> GetTeamDefenseElements()
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();
                var teamId = currentUser.TeamId ?? Guid.Empty;
                var forceType = currentUser.ForceType ?? "Blueland";

                if (teamId == Guid.Empty)
                {
                    return Ok(new { success = true, elements = new List<DefenseElement>(), count = 0 });
                }

                // Cache key includes teamId and forceType for proper isolation
                var cacheKey = $"DefenseElements_Team_{teamId}_{forceType}";
                
                // Try to get from cache first
                if (!_memoryCache.TryGetValue(cacheKey, out List<DefenseElement> elements))
                {
                    _logger.LogInformation("📥 Cache MISS for {CacheKey} - fetching from database", cacheKey);
                    
                    // Cache miss - fetch from database
                    var dal = new DefenseElementDAL(_context);
                    elements = await dal.GetTeamDefenseElementsAsync(teamId, forceType);
                    
                    // Store in cache with short expiration (10 seconds)
                    // This prevents duplicate calls during page initialization
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(10))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetSize(1) // For cache size management
                        .SetPriority(CacheItemPriority.Normal);
                    
                    _memoryCache.Set(cacheKey, elements, cacheOptions);
                }
                else
                {
                    _logger.LogDebug("⚡ Cache HIT for {CacheKey} - returning cached data", cacheKey);
                }

                return Ok(new
                {
                    success = true,
                    elements = elements,
                    count = elements.Count,
                    teamId = teamId,
                    forceType = forceType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting team defense elements");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get defense elements for a specific token
        /// </summary>
        [HttpGet("token/{tokenId}")]
        public async Task<IActionResult> GetDefenseElementsByToken(Guid tokenId)
        {
            try
            {
                var dal = new DefenseElementDAL(_context);
                var elements = await dal.GetDefenseElementsByTokenAsync(tokenId);

                // Calculate total defense strength
                var totalStrength = await dal.CalculateTokenDefenseStrengthAsync(tokenId);

                return Ok(new
                {
                    success = true,
                    elements = elements,
                    count = elements.Count,
                    totalDefenseStrength = totalStrength
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting defense elements for token {TokenId}", tokenId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create new defense element
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateDefenseElement([FromBody] DefenseElementRequest request)
        {
            try
            {
                var currentUser = _userSessionService.GetCurrentUser();

                var defenseElement = new DefenseElement
                {
                    ElementId = request.ElementId,
                    Category = request.Category,
                    Type = request.Type,
                    Coordinates = JsonSerializer.Serialize(request.Coordinates),
                    TokenId = request.TokenId,
                    Strength = request.Strength,
                    Effectiveness = request.Effectiveness,
                    Visibility = request.Visibility,
                    GameSessionId = request.GameSessionId,
                    CreatedByUserId = Guid.Parse(currentUser.ApplicationUserId),
                    TeamId = currentUser.TeamId,
                    Notes = request.Notes,
                    Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
                };

                var dal = new DefenseElementDAL(_context);
                var created = await dal.CreateDefenseElementAsync(defenseElement);

                _logger.LogInformation("Defense element created: {ElementId} by user {UserId}", 
                    created.ElementId, currentUser.ApplicationUserId);

                return Ok(new
                {
                    success = true,
                    message = "Defense element created successfully",
                    element = created,
                    id = created.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating defense element");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update defense element
        /// </summary>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDefenseElement(Guid id, [FromBody] DefenseElementRequest request)
        {
            try
            {
                var dal = new DefenseElementDAL(_context);
                var existing = await dal.GetDefenseElementByIdAsync(id);

                if (existing == null)
                {
                    return NotFound(new { success = false, message = "Defense element not found" });
                }

                // Update fields
                existing.Category = request.Category;
                existing.Type = request.Type;
                existing.Coordinates = JsonSerializer.Serialize(request.Coordinates);
                existing.TokenId = request.TokenId;
                existing.Strength = request.Strength;
                existing.Effectiveness = request.Effectiveness;
                existing.Visibility = request.Visibility;
                existing.Notes = request.Notes;
                existing.Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null;

                var updated = await dal.UpdateDefenseElementAsync(existing);

                _logger.LogInformation("Defense element updated: {ElementId}", updated.ElementId);

                return Ok(new
                {
                    success = true,
                    message = "Defense element updated successfully",
                    element = updated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating defense element {Id}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Associate defense element with token (assign responsibility)
        /// </summary>
        [HttpPost("associate/{defenseElementId}/token/{tokenId}")]
        public async Task<IActionResult> AssociateWithToken(Guid defenseElementId, Guid tokenId)
        {
            try
            {
                var dal = new DefenseElementDAL(_context);
                var result = await dal.AssociateWithTokenAsync(defenseElementId, tokenId);

                if (!result)
                {
                    return NotFound(new { success = false, message = "Defense element not found" });
                }

                // Calculate new total defense strength
                var totalStrength = await dal.CalculateTokenDefenseStrengthAsync(tokenId);

                _logger.LogInformation("Defense element {DefenseElementId} associated with token {TokenId}", 
                    defenseElementId, tokenId);

                return Ok(new
                {
                    success = true,
                    message = "Defense element associated with token successfully",
                    totalDefenseStrength = totalStrength
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error associating defense element {DefenseElementId} with token {TokenId}", 
                    defenseElementId, tokenId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Dissociate defense element from token
        /// </summary>
        [HttpPost("dissociate/{defenseElementId}")]
        public async Task<IActionResult> DissociateFromToken(Guid defenseElementId)
        {
            try
            {
                var dal = new DefenseElementDAL(_context);
                var result = await dal.DissociateFromTokenAsync(defenseElementId);

                if (!result)
                {
                    return NotFound(new { success = false, message = "Defense element not found" });
                }

                _logger.LogInformation("Defense element {DefenseElementId} dissociated from token", defenseElementId);

                return Ok(new
                {
                    success = true,
                    message = "Defense element dissociated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dissociating defense element {DefenseElementId}", defenseElementId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete defense element - tries both Id (primary key) and ElementId (GUID)
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteDefenseElement(Guid id)
        {
            try
            {
                // First try to find by primary key Id
                var element = await _context.DefenseElements.FindAsync(id);
                
                // If not found, try by ElementId (GUID from frontend)
                if (element == null)
                {
                    _logger.LogInformation("Element not found by Id, trying ElementId: {Id}", id);
                    element = await _context.DefenseElements
                        .FirstOrDefaultAsync(d => d.ElementId == id);
                }

                if (element == null)
                {
                    _logger.LogWarning("Defense element {Id} not found by Id or ElementId", id);
                    return NotFound(new { success = false, message = "Defense element not found" });
                }

                // Hard delete
                _context.DefenseElements.Remove(element);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Defense element deleted successfully - Id: {Id}, ElementId: {ElementId}", element.Id, element.ElementId);

                return Ok(new
                {
                    success = true,
                    message = "Defense element deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting defense element {Id}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Calculate total defense strength for a token
        /// </summary>
        [HttpGet("strength/{tokenId}")]
        public async Task<IActionResult> GetTokenDefenseStrength(Guid tokenId)
        {
            try
            {
                var dal = new DefenseElementDAL(_context);
                var totalStrength = await dal.CalculateTokenDefenseStrengthAsync(tokenId);

                return Ok(new
                {
                    success = true,
                    tokenId = tokenId,
                    totalDefenseStrength = totalStrength
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating defense strength for token {TokenId}", tokenId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get defense elements by category
        /// </summary>
        [HttpGet("category/{gameSessionId}/{category}")]
        public async Task<IActionResult> GetDefenseElementsByCategory(Guid gameSessionId, string category)
        {
            try
            {
                var dal = new DefenseElementDAL(_context);
                var elements = await dal.GetDefenseElementsByCategoryAsync(gameSessionId, category);

                return Ok(new
                {
                    success = true,
                    category = category,
                    elements = elements,
                    count = elements.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting defense elements by category {Category}", category);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request model for defense element creation/update
    /// </summary>
    public class DefenseElementRequest
    {
        public Guid ElementId { get; set; } 
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double[][] Coordinates { get; set; } = Array.Empty<double[]>();
        public Guid? TokenId { get; set; }
        public int Strength { get; set; } = 100;
        public double Effectiveness { get; set; } = 1.0;
        public string Visibility { get; set; } = "friendly";
        public Guid? GameSessionId { get; set; }
        public string? Notes { get; set; }
        public object? Metadata { get; set; }
    }
}

