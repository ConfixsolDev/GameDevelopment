using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TechWebSol.Data;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services.TokenManagement;

namespace TechWebSol.Controllers.TokenManagement
{
    [AuthorizeDynamic]
    [ApiController]
    [Route("api/[controller]")]
    public class TokenManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TokenManagementController> _logger;

        public TokenManagementController(ApplicationDbContext context, ILogger<TokenManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Create a complete token with all related data
        /// </summary>
        [HttpPost("create-complete")]
        public async Task<ActionResult<Token>> CreateCompleteToken([FromBody] CompleteTokenRequest request)
        {
            // Use EF Core’s execution strategy
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            try
            {
                // Execute everything inside a retriable unit
                return await executionStrategy.ExecuteAsync(async () =>
                {
                    // result to capture the response you want to return
                    ActionResult<Token> actionResult;

                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Create the main token
                        var token = new Token
                        {
                            Id = request.Token.Id,
                            Name = request.Token.Name,
                            CreatedAt = request.Token.CreatedAt,
                            TrainingConsistency = request.Token.TrainingConsistency,
                            Description = request.Token.Description,
                            Category = request.Token.Category,
                            IsActive = request.Token.IsActive,
                            UsageCount = request.Token.UsageCount,
                            LastUsed = request.Token.LastUsed,
                            CreatedBy = request.Token.CreatedBy,
                            Notes = request.Token.Notes
                        };

                        _context.Tokens.Add(token);

                        // Create the signature if provided
                        if (request.Signature != null)
                        {
                            var signature = new TokenSignature
                            {
                                TokenId = token.Id,
                                TouchCount = request.Signature.TouchCount,
                                Timestamp = request.Signature.Timestamp,
                                TokenHash = request.Signature.TokenHash,
                                OriginalTouches = request.Signature.OriginalTouches
                            };
                            _context.TokenSignatures.Add(signature);
                            _context.SaveChanges(); // Save to get the signature ID

                            // Create related entities
                            if (request.Stability != null)
                            {
                                _context.StabilityInfo.Add(new StabilityInfo
                                {
                                    TokenSignatureId = signature.Id,
                                    IsStabilized = request.Stability.IsStabilized,
                                    GeneratedAt = request.Stability.GeneratedAt,
                                    SampleCount = request.Stability.SampleCount
                                });
                            }

                            if (request.TouchGeometry != null)
                            {
                                _context.TouchGeometry.Add(new TouchGeometry
                                {
                                    TokenSignatureId = signature.Id,
                                    HasRadius = request.TouchGeometry.HasRadius,
                                    HasRotation = request.TouchGeometry.HasRotation,
                                    RadiusValues = request.TouchGeometry.RadiusValues,
                                    RotationValues = request.TouchGeometry.RotationValues,
                                    AvgRadius = request.TouchGeometry.AvgRadius,
                                    AvgRotation = request.TouchGeometry.AvgRotation,
                                    RadiusVariance = request.TouchGeometry.RadiusVariance
                                });
                            }

                            if (request.TouchPattern != null)
                            {
                                _context.TouchPatterns.Add(new TouchPattern
                                {
                                    TokenSignatureId = signature.Id,
                                    Type = request.TouchPattern.Type,
                                    Complexity = request.TouchPattern.Complexity,
                                    Distances = request.TouchPattern.Distances,
                                    DistancePairs = request.TouchPattern.DistancePairs,
                                    AvgDistance = request.TouchPattern.AvgDistance,
                                    MinDistance = request.TouchPattern.MinDistance,
                                    MaxDistance = request.TouchPattern.MaxDistance,
                                    DistanceRange = request.TouchPattern.DistanceRange,
                                    DistanceVariance = request.TouchPattern.DistanceVariance,
                                    DistanceSignature = request.TouchPattern.DistanceSignature,
                                    AngleSpread = request.TouchPattern.AngleSpread,
                                    GeometricCenter = request.TouchPattern.GeometricCenter
                                });
                            }

                            if (request.MultiTouchGeometry != null)
                            {
                                _context.MultiTouchGeometry.Add(new MultiTouchGeometry
                                {
                                    TokenSignatureId = signature.Id,
                                    AspectRatio = request.MultiTouchGeometry.AspectRatio,
                                    BoundingBoxWidth = request.MultiTouchGeometry.BoundingBoxWidth,
                                    BoundingBoxHeight = request.MultiTouchGeometry.BoundingBoxHeight,
                                    BoundingBoxArea = request.MultiTouchGeometry.BoundingBoxArea,
                                    CenterX = request.MultiTouchGeometry.CenterX,
                                    CenterY = request.MultiTouchGeometry.CenterY,
                                    Spread = request.MultiTouchGeometry.Spread,
                                    Density = request.MultiTouchGeometry.Density
                                });
                            }
                        }

                        // Save all changes in one call
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        // Return the created token using CreatedAtAction
                        actionResult = CreatedAtAction(nameof(GetToken), new { id = token.Id }, token);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }

                    return actionResult;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating complete token");
                return StatusCode(500, "Internal server error");
            }
        }


        /// <summary>
        /// Get token with all related data
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CompleteTokenResponse>> GetToken(long id)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.Stability)
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.TouchProperties)
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.TouchPattern)
                    .Include(t => t.Signature)
                        .ThenInclude(s => s.MultiTouchGeometry)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (token == null)
                    return NotFound();

                var response = new CompleteTokenResponse
                {
                    Token = token,
                    Signature = token.Signature,
                    Stability = token.Signature?.Stability,
                    TouchGeometry = token.Signature?.TouchProperties,
                    TouchPattern = token.Signature?.TouchPattern,
                    MultiTouchGeometry = token.Signature?.MultiTouchGeometry
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token {TokenId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all tokens with basic information
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Token>>> GetTokens()
        {
            try
            {

                var tokens = await _context.Tokens
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.Stability)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchProperties)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.TouchPattern)
                .Include(t => t.Signature)
                    .ThenInclude(s => s!.MultiTouchGeometry)
                .Where(t => t.IsActive)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();


                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tokens");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete token and all related data
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToken(long id)
        {
            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                // Execute everything in a retryable unit of work
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var token = await _context.Tokens
                            .Include(t => t.Signature)
                            .FirstOrDefaultAsync(t => t.Id == id);

                        if (token == null)
                        {
                            return (IActionResult)NotFound();
                        }

                        // Delete related signature and dependent data
                        if (token.Signature != null)
                        {
                            await _context.StabilityInfo
                                .Where(s => s.TokenSignatureId == token.Signature.Id)
                                .ExecuteDeleteAsync();

                            await _context.TouchGeometry
                                .Where(tg => tg.TokenSignatureId == token.Signature.Id)
                                .ExecuteDeleteAsync();

                            await _context.TouchPatterns
                                .Where(tp => tp.TokenSignatureId == token.Signature.Id)
                                .ExecuteDeleteAsync();

                            await _context.MultiTouchGeometry
                                .Where(mtg => mtg.TokenSignatureId == token.Signature.Id)
                                .ExecuteDeleteAsync();

                            _context.TokenSignatures.Remove(token.Signature);
                        }

                        // Delete map markers and the token itself
                        await _context.MapMarkers
                            .Where(m => m.TokenId == id)
                            .ExecuteDeleteAsync();

                        _context.Tokens.Remove(token);
                        await _context.SaveChangesAsync();

                        // Commit the transaction
                        await transaction.CommitAsync();
                        return (IActionResult)NoContent();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting token {TokenId}", id);
                return StatusCode(500, "Internal server error");
            }
        }


        // Request/Response DTOs
        public class CompleteTokenRequest
        {
            public Token Token { get; set; } = null!;
            public TokenSignature? Signature { get; set; }
            public StabilityInfo? Stability { get; set; }
            public TouchGeometry? TouchGeometry { get; set; }
            public TouchPattern? TouchPattern { get; set; }
            public MultiTouchGeometry? MultiTouchGeometry { get; set; }
        }

        public class CompleteTokenResponse
        {
            public Token Token { get; set; } = null!;
            public TokenSignature? Signature { get; set; }
            public StabilityInfo? Stability { get; set; }
            public TouchGeometry? TouchGeometry { get; set; }
            public TouchPattern? TouchPattern { get; set; }
            public MultiTouchGeometry? MultiTouchGeometry { get; set; }
        }
    }
}
