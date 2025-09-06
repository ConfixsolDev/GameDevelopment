using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;

namespace TechWebSol.Controllers.TokenManagement
{
    /// <summary>
    /// Unified Token Controller - Single API for all token operations
    /// Uses the TokenIdentificationDAL for consistent behavior across all systems
    /// </summary>
    [AuthorizeDynamic]
    [ApiController]
    [Route("api/[controller]")]
    public class UnifiedTokenController : ControllerBase
    {
        private readonly TokenIdentificationDAL _tokenDAL;
        private readonly ILogger<UnifiedTokenController> _logger;

        public UnifiedTokenController(TokenIdentificationDAL tokenDAL, ILogger<UnifiedTokenController> logger)
        {
            _tokenDAL = tokenDAL;
            _logger = logger;
        }

        /// <summary>
        /// UNIFIED TOKEN IDENTIFICATION - Single API for all identification needs
        /// Used by map, testing, production, and all other systems
        /// </summary>
        [HttpPost("identify")]
        public async Task<ActionResult<UnifiedTokenIdentificationResult>> IdentifyToken([FromBody] UnifiedTokenIdentificationRequest request)
        {
            try
            {
                if (request?.TouchPoints == null || request.TouchPoints.Length < 2)
                {
                    return BadRequest("At least 2 touch points are required");
                }

                var result = await _tokenDAL.IdentifyTokenAsync(
                    request.TouchPoints, 
                    request.ConfidenceThreshold ?? 70.0,
                    request.PreferSimplified ?? true
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error identifying token");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// UNIFIED TOKEN SAVE - Single API for all save operations
        /// Handles both creation and updates across both systems
        /// Automatically associates with current user's team
        /// </summary>
        [HttpPost("save")]
        public async Task<ActionResult<UnifiedSaveResult>> SaveToken([FromBody] UnifiedTokenSaveRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("Token name is required");
                }

                var result = await _tokenDAL.SaveTokenAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving token");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get team's grouped token list for dropdowns
        /// Returns tokens organized by groups assigned to the current user's team
        /// </summary>
        [HttpGet("team-tokens")]
        public async Task<ActionResult<List<GroupedTeamTokenInfo>>> GetTeamTokens()
        {
            try
            {
                var result = await _tokenDAL.GetTeamTokensAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team tokens");
                return StatusCode(500, "Internal server error");
            }
        }

    }

    /// <summary>
    /// Request for token identification
    /// </summary>
    public class UnifiedTokenIdentificationRequest
    {
        public double[][] TouchPoints { get; set; } = null!;
        public double? ConfidenceThreshold { get; set; }
        public bool? PreferSimplified { get; set; }
    }


}
