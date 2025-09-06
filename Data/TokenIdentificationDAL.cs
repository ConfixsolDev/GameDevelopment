using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;
using TechWebSol.Services.TokenManagement;
using System.Text.Json;

namespace TechWebSol.Data
{
    /// <summary>
    /// Unified Data Access Layer for Token Identification
    /// Provides a single, consistent API for token identification across all systems
    /// Works with both complex and simplified token systems
    /// </summary>
    public class TokenIdentificationDAL
    {
        private readonly ApplicationDbContext _complexContext;
        private readonly SimplifiedApplicationDbContext _simplifiedContext;
        private readonly IPatternMatchingService _complexPatternService;
        private readonly ISimplifiedPatternMatchingService _simplifiedPatternService;
        private readonly ILogger<TokenIdentificationDAL> _logger;

        public TokenIdentificationDAL(
            ApplicationDbContext complexContext,
            SimplifiedApplicationDbContext simplifiedContext,
            IPatternMatchingService complexPatternService,
            ISimplifiedPatternMatchingService simplifiedPatternService,
            ILogger<TokenIdentificationDAL> logger)
        {
            _complexContext = complexContext;
            _simplifiedContext = simplifiedContext;
            _complexPatternService = complexPatternService;
            _simplifiedPatternService = simplifiedPatternService;
            _logger = logger;
        }

        /// <summary>
        /// Unified token identification method
        /// Automatically detects the best matching system and returns consistent results
        /// </summary>
        public async Task<UnifiedTokenIdentificationResult> IdentifyTokenAsync(
            double[][] touchPoints, 
            double confidenceThreshold = 70.0,
            bool preferSimplified = true)
        {
            try
            {
                _logger.LogInformation("Starting unified token identification with {PointCount} touch points", 
                    touchPoints.Length);

                if (touchPoints.Length < 2)
                {
                    return new UnifiedTokenIdentificationResult
                    {
                        Success = false,
                        Message = "At least 2 touch points are required",
                        SystemUsed = "none"
                    };
                }

                // Try simplified system first (faster and more reliable for basic patterns)
                if (preferSimplified)
                {
                    var simplifiedResult = await TrySimplifiedIdentification(touchPoints, confidenceThreshold);
                    if (simplifiedResult.Success)
                    {
                        return simplifiedResult;
                    }
                }

                // Fallback to complex system
                var complexResult = await TryComplexIdentification(touchPoints, confidenceThreshold);
                if (complexResult.Success)
                {
                    return complexResult;
                }

                // If both systems fail, return the best result
                var simplifiedFallback = await TrySimplifiedIdentification(touchPoints, confidenceThreshold);
                return simplifiedFallback;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during unified token identification");
                return new UnifiedTokenIdentificationResult
                {
                    Success = false,
                    Message = "Error during token identification",
                    SystemUsed = "error"
                };
            }
        }

        /// <summary>
        /// Try identification using simplified system
        /// </summary>
        private async Task<UnifiedTokenIdentificationResult> TrySimplifiedIdentification(
            double[][] touchPoints, 
            double confidenceThreshold)
        {
            try
            {
                // Calculate geometric data
                var geometricData = await _simplifiedPatternService.CalculateGeometricDataAsync(touchPoints);
                
                // Identify token
                var result = await _simplifiedPatternService.IdentifyTokenAsync(geometricData, confidenceThreshold);
                
                return new UnifiedTokenIdentificationResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    Confidence = result.Confidence,
                    SystemUsed = "simplified",
                    MatchedTokenId = result.MatchedToken?.Id,
                    MatchedTokenName = result.MatchedToken?.Name,
                    AllMatches = result.AllMatches.Select(m => new UnifiedTokenMatch
                    {
                        TokenId = m.TokenId,
                        TokenName = m.TokenName,
                        Confidence = m.Confidence,
                        DistanceSimilarity = m.DistanceSimilarity,
                        ShapeSimilarity = m.ShapeSimilarity,
                        TimingSimilarity = m.TimingSimilarity,
                        GeometricSimilarity = m.GeometricSimilarity,
                        MatchFactors = m.MatchFactors
                    }).ToList(),
                    GeometricData = geometricData
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Simplified identification failed, trying complex system");
                return new UnifiedTokenIdentificationResult
                {
                    Success = false,
                    Message = "Simplified identification failed",
                    SystemUsed = "simplified_failed"
                };
            }
        }

        /// <summary>
        /// Try identification using complex system
        /// </summary>
        private async Task<UnifiedTokenIdentificationResult> TryComplexIdentification(
            double[][] touchPoints, 
            double confidenceThreshold)
        {
            try
            {
                // Convert touch points to complex signature format
                var signature = await ConvertToComplexSignature(touchPoints);
                
                // Identify token
                var result = await _complexPatternService.IdentifyTokenAsync(signature, confidenceThreshold);
                
                return new UnifiedTokenIdentificationResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    Confidence = result.Confidence,
                    SystemUsed = "complex",
                    MatchedTokenId = result.MatchedToken?.Id,
                    MatchedTokenName = result.MatchedToken?.Name,
                    AllMatches = result.AllMatches.Select(m => new UnifiedTokenMatch
                    {
                        TokenId = m.TokenId,
                        TokenName = m.TokenName,
                        Confidence = m.Confidence,
                        DistanceSimilarity = m.DistanceSimilarity,
                        ShapeSimilarity = m.ShapeSimilarity,
                        TimingSimilarity = m.TimingSimilarity,
                        GeometricSimilarity = m.GeometricSimilarity,
                        MatchFactors = m.MatchFactors
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Complex identification failed");
                return new UnifiedTokenIdentificationResult
                {
                    Success = false,
                    Message = "Complex identification failed",
                    SystemUsed = "complex_failed"
                };
            }
        }

        /// <summary>
        /// Convert touch points to complex signature format
        /// </summary>
        private async Task<TokenSignature> ConvertToComplexSignature(double[][] touchPoints)
        {
            // This is a simplified conversion - in a real system, you'd want more sophisticated analysis
            var signature = new TokenSignature
            {
                TouchCount = touchPoints.Length,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Calculate basic geometric properties
            if (touchPoints.Length >= 2)
            {
                var distances = CalculateDistances(touchPoints);
                var center = CalculateCenter(touchPoints);
                
                // Create touch pattern
                signature.TouchPattern = new TouchPattern
                {
                    Type = touchPoints.Length == 2 ? "line" : "polygon",
                    Complexity = touchPoints.Length,
                    Distances = JsonSerializer.Serialize(distances),
                    AvgDistance = (decimal)distances.Average(),
                    MinDistance = (decimal)distances.Min(),
                    MaxDistance = (decimal)distances.Max(),
                    DistanceRange = (decimal)(distances.Max() - distances.Min()),
                    DistanceVariance = (decimal)CalculateVariance(distances),
                    GeometricCenter = JsonSerializer.Serialize(new { X = center.X, Y = center.Y })
                };

                // Create multi-touch geometry
                if (touchPoints.Length >= 3)
                {
                    var bounds = CalculateBoundingBox(touchPoints);
                    signature.MultiTouchGeometry = new MultiTouchGeometry
                    {
                        AspectRatio = (decimal)(bounds.Width / bounds.Height),
                        BoundingBoxWidth = (decimal)bounds.Width,
                        BoundingBoxHeight = (decimal)bounds.Height,
                        BoundingBoxArea = (decimal)(bounds.Width * bounds.Height),
                        CenterX = (decimal)center.X,
                        CenterY = (decimal)center.Y,
                        Spread = (decimal)CalculateSpread(touchPoints),
                        Density = (decimal)(touchPoints.Length / (bounds.Width * bounds.Height))
                    };
                }
            }

            return signature;
        }

        /// <summary>
        /// Calculate distances between all touch points
        /// </summary>
        private double[] CalculateDistances(double[][] points)
        {
            var distances = new List<double>();
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    var dx = points[j][0] - points[i][0];
                    var dy = points[j][1] - points[i][1];
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    distances.Add(distance);
                }
            }
            return distances.ToArray();
        }

        /// <summary>
        /// Calculate center point of all touch points
        /// </summary>
        private (double X, double Y) CalculateCenter(double[][] points)
        {
            var centerX = points.Average(p => p[0]);
            var centerY = points.Average(p => p[1]);
            return (centerX, centerY);
        }

        /// <summary>
        /// Calculate bounding box of touch points
        /// </summary>
        private (double Width, double Height) CalculateBoundingBox(double[][] points)
        {
            var minX = points.Min(p => p[0]);
            var maxX = points.Max(p => p[0]);
            var minY = points.Min(p => p[1]);
            var maxY = points.Max(p => p[1]);
            return (maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Calculate spread of touch points
        /// </summary>
        private double CalculateSpread(double[][] points)
        {
            var center = CalculateCenter(points);
            var distances = points.Select(p => 
                Math.Sqrt(Math.Pow(p[0] - center.X, 2) + Math.Pow(p[1] - center.Y, 2))).ToArray();
            return distances.Average();
        }

        /// <summary>
        /// Calculate variance of distances
        /// </summary>
        private double CalculateVariance(double[] values)
        {
            if (values.Length == 0) return 0;
            var mean = values.Average();
            var squaredDiffs = values.Select(v => Math.Pow(v - mean, 2));
            return squaredDiffs.Average();
        }

        /// <summary>
        /// Get token by ID from either system
        /// </summary>
        public async Task<UnifiedTokenInfo?> GetTokenByIdAsync(long tokenId)
        {
            try
            {
                // Try simplified system first
                var simplifiedToken = await _simplifiedContext.Tokens
                    .Include(t => t.Signature)
                    .FirstOrDefaultAsync(t => t.Id == tokenId);

                if (simplifiedToken != null)
                {
                    return new UnifiedTokenInfo
                    {
                        Id = simplifiedToken.Id,
                        Name = simplifiedToken.Name,
                        Description = simplifiedToken.Description,
                        Category = simplifiedToken.Category,
                        IsActive = simplifiedToken.IsActive,
                        CreatedAt = simplifiedToken.CreatedAt,
                        LastUsed = simplifiedToken.LastUsed,
                        UsageCount = simplifiedToken.UsageCount,
                        System = "simplified",
                        TouchCount = simplifiedToken.Signature?.TouchCount ?? 0
                    };
                }

                // Try complex system
                var complexToken = await _complexContext.Tokens
                    .Include(t => t.Signature)
                    .FirstOrDefaultAsync(t => t.Id == tokenId);

                if (complexToken != null)
                {
                    return new UnifiedTokenInfo
                    {
                        Id = complexToken.Id,
                        Name = complexToken.Name,
                        Description = complexToken.Description,
                        Category = complexToken.Category,
                        IsActive = complexToken.IsActive,
                        CreatedAt = complexToken.CreatedAt,
                        LastUsed = complexToken.LastUsed,
                        UsageCount = complexToken.UsageCount,
                        System = "complex",
                        TouchCount = complexToken.Signature?.TouchCount ?? 0
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token by ID {TokenId}", tokenId);
                return null;
            }
        }

        /// <summary>
        /// Get all active tokens from both systems
        /// </summary>
        public async Task<List<UnifiedTokenInfo>> GetAllActiveTokensAsync()
        {
            var tokens = new List<UnifiedTokenInfo>();

            try
            {
                // Get simplified tokens
                var simplifiedTokens = await _simplifiedContext.Tokens
                    .Include(t => t.Signature)
                    .Where(t => t.IsActive)
                    .ToListAsync();

                tokens.AddRange(simplifiedTokens.Select(t => new UnifiedTokenInfo
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Category = t.Category,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    LastUsed = t.LastUsed,
                    UsageCount = t.UsageCount,
                    System = "simplified",
                    TouchCount = t.Signature?.TouchCount ?? 0
                }));

                // Get complex tokens
                var complexTokens = await _complexContext.Tokens
                    .Include(t => t.Signature)
                    .Where(t => t.IsActive)
                    .ToListAsync();

                tokens.AddRange(complexTokens.Select(t => new UnifiedTokenInfo
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Category = t.Category,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    LastUsed = t.LastUsed,
                    UsageCount = t.UsageCount,
                    System = "complex",
                    TouchCount = t.Signature?.TouchCount ?? 0
                }));

                return tokens.OrderBy(t => t.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all active tokens");
                return tokens;
            }
        }

        /// <summary>
        /// UNIFIED SAVE FUNCTION - The single point for all token saving operations
        /// Handles both creation and updates across both systems
        /// </summary>
        public async Task<UnifiedSaveResult> SaveTokenAsync(UnifiedTokenSaveRequest request)
        {
            try
            {
                _logger.LogInformation("Starting unified token save operation for token: {TokenName}", request.Name);

                // Validate request
                var validationResult = ValidateSaveRequest(request);
                if (!validationResult.IsValid)
                {
                    return new UnifiedSaveResult
                    {
                        Success = false,
                        Message = validationResult.ErrorMessage,
                        TokenId = null
                    };
                }

                // Determine which system to use based on request or auto-detect
                var systemToUse = DetermineSystemToUse(request);
                
                if (systemToUse == "simplified")
                {
                    return await SaveToSimplifiedSystem(request);
                }
                else
                {
                    return await SaveToComplexSystem(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during unified token save operation");
                return new UnifiedSaveResult
                {
                    Success = false,
                    Message = "Error saving token",
                    TokenId = null
                };
            }
        }

        /// <summary>
        /// Save token to simplified system
        /// </summary>
        private async Task<UnifiedSaveResult> SaveToSimplifiedSystem(UnifiedTokenSaveRequest request)
        {
            try
            {
                using var transaction = await _simplifiedContext.Database.BeginTransactionAsync();
                
                try
                {
                    SimplifiedToken token;
                    SimplifiedTokenSignature signature;

                    if (request.TokenId.HasValue)
                    {
                        // Update existing token
                        token = await _simplifiedContext.Tokens
                            .Include(t => t.Signature)
                            .FirstOrDefaultAsync(t => t.Id == request.TokenId.Value);

                        if (token == null)
                        {
                            return new UnifiedSaveResult
                            {
                                Success = false,
                                Message = "Token not found for update",
                                TokenId = null
                            };
                        }

                        // Update token properties
                        token.Name = request.Name;
                        token.Description = request.Description;
                        token.Category = request.Category;
                        token.IsActive = request.IsActive;

                        // Update or create signature
                        if (token.Signature != null)
                        {
                            signature = token.Signature;
                        }
                        else
                        {
                            signature = new SimplifiedTokenSignature
                            {
                                TokenId = token.Id,
                                Token = token
                            };
                            token.Signature = signature;
                        }
                    }
                    else
                    {
                        // Create new token
                        var tokenId = request.TokenId ?? GenerateTokenId();
                        
                        token = new SimplifiedToken
                        {
                            Id = tokenId,
                            Name = request.Name,
                            Description = request.Description,
                            Category = request.Category,
                            IsActive = request.IsActive,
                            CreatedAt = DateTime.UtcNow,
                            UsageCount = 0
                        };

                        signature = new SimplifiedTokenSignature
                        {
                            TokenId = tokenId,
                            Token = token
                        };

                        token.Signature = signature;
                        _simplifiedContext.Tokens.Add(token);
                    }

                    // Update signature with geometric data
                    if (request.TouchPoints != null && request.TouchPoints.Length >= 2)
                    {
                        var geometricData = await _simplifiedPatternService.CalculateGeometricDataAsync(request.TouchPoints);
                        
                        signature.TouchCount = request.TouchPoints.Length;
                        signature.Distances = JsonSerializer.Serialize(geometricData.Distances);
                        signature.Angles = JsonSerializer.Serialize(geometricData.Angles);
                        signature.Center = JsonSerializer.Serialize(geometricData.Center);

                        if (token.Signature == null)
                        {
                            _simplifiedContext.TokenSignatures.Add(signature);
                        }
                    }

                    await _simplifiedContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully saved token to simplified system: {TokenId}", token.Id);

                    return new UnifiedSaveResult
                    {
                        Success = true,
                        Message = "Token saved successfully to simplified system",
                        TokenId = token.Id,
                        SystemUsed = "simplified"
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving token to simplified system");
                return new UnifiedSaveResult
                {
                    Success = false,
                    Message = "Error saving token to simplified system",
                    TokenId = null
                };
            }
        }

        /// <summary>
        /// Save token to complex system
        /// </summary>
        private async Task<UnifiedSaveResult> SaveToComplexSystem(UnifiedTokenSaveRequest request)
        {
            try
            {
                using var transaction = await _complexContext.Database.BeginTransactionAsync();
                
                try
                {
                    Token token;
                    TokenSignature signature;

                    if (request.TokenId.HasValue)
                    {
                        // Update existing token
                        token = await _complexContext.Tokens
                            .Include(t => t.Signature)
                            .FirstOrDefaultAsync(t => t.Id == request.TokenId.Value);

                        if (token == null)
                        {
                            return new UnifiedSaveResult
                            {
                                Success = false,
                                Message = "Token not found for update",
                                TokenId = null
                            };
                        }

                        // Update token properties
                        token.Name = request.Name;
                        token.Description = request.Description;
                        token.Category = request.Category;
                        token.IsActive = request.IsActive;

                        // Update or create signature
                        if (token.Signature != null)
                        {
                            signature = token.Signature;
                        }
                        else
                        {
                            signature = new TokenSignature
                            {
                                TokenId = token.Id,
                                Token = token
                            };
                            token.Signature = signature;
                        }
                    }
                    else
                    {
                        // Create new token
                        var tokenId = request.TokenId ?? GenerateTokenId();
                        
                        token = new Token
                        {
                            Id = tokenId,
                            Name = request.Name,
                            Description = request.Description,
                            Category = request.Category,
                            IsActive = request.IsActive,
                            CreatedAt = DateTime.UtcNow,
                            UsageCount = 0,
                            TrainingConsistency = 0
                        };

                        signature = new TokenSignature
                        {
                            TokenId = tokenId,
                            Token = token
                        };

                        token.Signature = signature;
                        _complexContext.Tokens.Add(token);
                    }

                    // Update signature with complex data
                    if (request.TouchPoints != null && request.TouchPoints.Length >= 2)
                    {
                        signature = await ConvertToComplexSignature(request.TouchPoints);
                        signature.TokenId = token.Id;
                        signature.Token = token;

                        if (token.Signature == null)
                        {
                            _complexContext.TokenSignatures.Add(signature);
                        }
                    }

                    await _complexContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully saved token to complex system: {TokenId}", token.Id);

                    return new UnifiedSaveResult
                    {
                        Success = true,
                        Message = "Token saved successfully to complex system",
                        TokenId = token.Id,
                        SystemUsed = "complex"
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving token to complex system");
                return new UnifiedSaveResult
                {
                    Success = false,
                    Message = "Error saving token to complex system",
                    TokenId = null
                };
            }
        }

        /// <summary>
        /// Validate save request
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateSaveRequest(UnifiedTokenSaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return (false, "Token name is required");
            }

            if (request.TouchPoints != null && request.TouchPoints.Length < 2)
            {
                return (false, "At least 2 touch points are required");
            }

            if (request.TouchPoints != null && request.TouchPoints.Length > 5)
            {
                return (false, "Maximum 5 touch points allowed");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Determine which system to use for saving
        /// </summary>
        private string DetermineSystemToUse(UnifiedTokenSaveRequest request)
        {
            // If explicitly specified, use that
            if (!string.IsNullOrEmpty(request.PreferredSystem))
            {
                return request.PreferredSystem;
            }

            // If updating existing token, use the same system
            if (request.TokenId.HasValue)
            {
                var existingToken = GetTokenByIdAsync(request.TokenId.Value).Result;
                if (existingToken != null)
                {
                    return existingToken.System;
                }
            }

            // Default to simplified system for new tokens (faster and more reliable)
            return "simplified";
        }

        /// <summary>
        /// Generate a unique token ID
        /// </summary>
        private long GenerateTokenId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Delete token from both systems
        /// </summary>
        public async Task<UnifiedSaveResult> DeleteTokenAsync(long tokenId)
        {
            try
            {
                var results = new List<UnifiedSaveResult>();

                // Try to delete from simplified system
                var simplifiedToken = await _simplifiedContext.Tokens.FindAsync(tokenId);
                if (simplifiedToken != null)
                {
                    _simplifiedContext.Tokens.Remove(simplifiedToken);
                    await _simplifiedContext.SaveChangesAsync();
                    results.Add(new UnifiedSaveResult
                    {
                        Success = true,
                        Message = "Token deleted from simplified system",
                        TokenId = tokenId,
                        SystemUsed = "simplified"
                    });
                }

                // Try to delete from complex system
                var complexToken = await _complexContext.Tokens.FindAsync(tokenId);
                if (complexToken != null)
                {
                    _complexContext.Tokens.Remove(complexToken);
                    await _complexContext.SaveChangesAsync();
                    results.Add(new UnifiedSaveResult
                    {
                        Success = true,
                        Message = "Token deleted from complex system",
                        TokenId = tokenId,
                        SystemUsed = "complex"
                    });
                }

                if (results.Any())
                {
                    return new UnifiedSaveResult
                    {
                        Success = true,
                        Message = $"Token deleted from {results.Count} system(s)",
                        TokenId = tokenId,
                        SystemUsed = string.Join(",", results.Select(r => r.SystemUsed))
                    };
                }

                return new UnifiedSaveResult
                {
                    Success = false,
                    Message = "Token not found in any system",
                    TokenId = tokenId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting token {TokenId}", tokenId);
                return new UnifiedSaveResult
                {
                    Success = false,
                    Message = "Error deleting token",
                    TokenId = tokenId
                };
            }
        }
    }

    /// <summary>
    /// Unified result for token identification
    /// </summary>
    public class UnifiedTokenIdentificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string SystemUsed { get; set; } = string.Empty;
        public long? MatchedTokenId { get; set; }
        public string? MatchedTokenName { get; set; }
        public List<UnifiedTokenMatch> AllMatches { get; set; } = new();
        public GeometricData? GeometricData { get; set; }
    }

    /// <summary>
    /// Unified token match information
    /// </summary>
    public class UnifiedTokenMatch
    {
        public long TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double DistanceSimilarity { get; set; }
        public double ShapeSimilarity { get; set; }
        public double TimingSimilarity { get; set; }
        public double GeometricSimilarity { get; set; }
        public List<string> MatchFactors { get; set; } = new();
    }

    /// <summary>
    /// Unified token information
    /// </summary>
    public class UnifiedTokenInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public int UsageCount { get; set; }
        public string System { get; set; } = string.Empty;
        public int TouchCount { get; set; }
    }

    /// <summary>
    /// Unified token save request - Single request format for all save operations
    /// </summary>
    public class UnifiedTokenSaveRequest
    {
        /// <summary>
        /// Token ID for updates, null for new tokens
        /// </summary>
        public long? TokenId { get; set; }

        /// <summary>
        /// Token name (required)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Token description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Token category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Whether token is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Touch points for pattern creation/update
        /// </summary>
        public double[][]? TouchPoints { get; set; }

        /// <summary>
        /// Preferred system to use ("simplified" or "complex")
        /// If not specified, will auto-detect based on existing token or default to simplified
        /// </summary>
        public string? PreferredSystem { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Unified save result
    /// </summary>
    public class UnifiedSaveResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long? TokenId { get; set; }
        public string? SystemUsed { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
