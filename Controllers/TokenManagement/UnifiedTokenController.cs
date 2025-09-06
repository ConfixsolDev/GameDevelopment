using Microsoft.AspNetCore.Mvc;
using TechWebSol.Data;
using TechWebSol.Filters;

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
        /// Get token by ID from either system
        /// </summary>
        [HttpGet("{tokenId}")]
        public async Task<ActionResult<UnifiedTokenInfo>> GetToken(long tokenId)
        {
            try
            {
                var token = await _tokenDAL.GetTokenByIdAsync(tokenId);
                if (token == null)
                {
                    return NotFound("Token not found");
                }

                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token {TokenId}", tokenId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all active tokens from both systems
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<List<UnifiedTokenInfo>>> GetAllTokens()
        {
            try
            {
                var tokens = await _tokenDAL.GetAllActiveTokensAsync();
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tokens");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete token from both systems
        /// </summary>
        [HttpDelete("{tokenId}")]
        public async Task<ActionResult<UnifiedSaveResult>> DeleteToken(long tokenId)
        {
            try
            {
                var result = await _tokenDAL.DeleteTokenAsync(tokenId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting token {TokenId}", tokenId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Test token identification with sample data
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<UnifiedTokenIdentificationResult>> TestIdentification([FromBody] TestTokenRequest request)
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
                _logger.LogError(ex, "Error testing token identification");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a test token for development/testing
        /// </summary>
        [HttpPost("create-test")]
        public async Task<ActionResult<UnifiedSaveResult>> CreateTestToken([FromBody] CreateTestTokenRequest request)
        {
            try
            {
                var saveRequest = new UnifiedTokenSaveRequest
                {
                    Name = request.Name ?? $"Test Token {DateTime.Now:yyyyMMddHHmmss}",
                    Description = request.Description ?? "Test token created for development",
                    Category = request.Category ?? "Test",
                    IsActive = true,
                    TouchPoints = request.TouchPoints,
                    PreferredSystem = request.PreferredSystem ?? "simplified"
                };

                var result = await _tokenDAL.SaveTokenAsync(saveRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test token");
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

    /// <summary>
    /// Request for testing token identification
    /// </summary>
    public class TestTokenRequest
    {
        public double[][] TouchPoints { get; set; } = null!;
        public double? ConfidenceThreshold { get; set; }
        public bool? PreferSimplified { get; set; }
    }

    /// <summary>
    /// Request for creating test tokens
    /// </summary>
    public class CreateTestTokenRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public double[][] TouchPoints { get; set; } = null!;
        public string? PreferredSystem { get; set; }
    }
}
