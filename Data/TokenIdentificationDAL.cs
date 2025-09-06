using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.Services.TokenManagement;
using System.Text.Json;

namespace TechWebSol.Data
{
    /// <summary>
    /// Unified Data Access Layer for Token Identification
    /// Provides a single, consistent API for token identification across all systems
    /// Works with both complex and simplified token systems using single database context
    /// Team-based isolation ensures tokens are only accessible within the same team
    /// </summary>
    public class TokenIdentificationDAL
    {
        private readonly ApplicationDbContext _context;
        private readonly IPatternMatchingService _patternService;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<TokenIdentificationDAL> _logger;

        public TokenIdentificationDAL(
            ApplicationDbContext context,
            IPatternMatchingService patternService,
            IUserSessionService userSessionService,
            ILogger<TokenIdentificationDAL> logger)
        {
            _context = context;
            _patternService = patternService;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's team ID (TeamCode + SubTeamCode)
        /// </summary>
        private string GetCurrentTeamId()
        {
            var currentUser = _userSessionService.GetCurrentUser();
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Get user details from database to get TeamCode and SubTeamCode
            var user = _context.Users.FirstOrDefault(u => u.Id == currentUser.ApplicationUserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return $"{user.TeamCode}_{user.SubTeamCode}";
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        private string GetCurrentUserId()
        {
            var currentUser = _userSessionService.GetCurrentUser();
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return currentUser.ApplicationUserId;
        }

        /// <summary>
        /// Get current user name
        /// </summary>
        private string GetCurrentUserName()
        {
            var currentUser = _userSessionService.GetCurrentUser();
            if (currentUser == null)
            {
                return "Unknown User";
            }

            return currentUser.FullName;
        }

        /// <summary>
        /// Unified token identification method
        /// Automatically detects the best matching system and returns consistent results
        /// Only searches within the current user's team
        /// </summary>
        public async Task<UnifiedTokenIdentificationResult> IdentifyTokenAsync(
            double[][] touchPoints, 
            double confidenceThreshold = 70.0,
            bool preferSimplified = true)
        {
            try
            {
                var teamId = GetCurrentTeamId();
                _logger.LogInformation("Starting unified token identification with {PointCount} touch points for team {TeamId}", 
                    touchPoints.Length, teamId);

                if (touchPoints.Length < 2)
                {
                    return new UnifiedTokenIdentificationResult
                    {
                        Success = false,
                        Message = "At least 2 touch points are required",
                        SystemUsed = "none"
                    };
                }

                // Get active game sessions and bindings for this team
                var activeSessionIds = await _context.GameSessions.Where(s => s.Status == "Active").Select(s => s.Id).ToListAsync();
                var boundGroupIds = await _context.TokenBindings
                    .Where(b => b.TeamId == teamId && activeSessionIds.Contains(b.GameSessionId) && b.IsActive)
                    .Select(b => b.TokenGroupId)
                    .ToListAsync();

                var teamTokens = await _context.Tokens
                    .Include(t => t.Signature)
                    .Where(t => boundGroupIds.Contains(t.TokenGroupId.Value) && t.IsActive)
                    .ToListAsync();

                if (!teamTokens.Any())
                {
                    return new UnifiedTokenIdentificationResult
                    {
                        Success = false,
                        Message = "No tokens found for this team in active game sessions",
                        Confidence = 0,
                        TokenId = null,
                        TokenName = null,
                        SystemUsed = "unified"
                    };
                }

                // Calculate geometric data for pattern matching
                var geometricData = await CalculateGeometricDataAsync(touchPoints);
                var signature = ConvertToTokenSignature(geometricData);

                // Use pattern matching service
                var result = await _patternService.IdentifyTokenAsync(signature, confidenceThreshold);
                
                if (result.Success)
                {
                    var matchedToken = teamTokens.FirstOrDefault(t => t.Id == result.MatchedToken?.Id);
                    if (matchedToken != null)
                    {
                        return new UnifiedTokenIdentificationResult
                        {
                            Success = true,
                            Message = "Token identified successfully",
                            Confidence = result.Confidence,
                            TokenId = matchedToken.Id,
                            TokenName = matchedToken.Name,
                            SystemUsed = "unified",
                            MatchDetails = result.AllMatches?.Select(m => new UnifiedTokenMatchDetail
                            {
                                TokenId = m.TokenId,
                                TokenName = m.TokenName,
                                Confidence = m.Confidence,
                                DistanceSimilarity = m.DistanceSimilarity,
                                AngleSimilarity = m.ShapeSimilarity, // Map ShapeSimilarity to AngleSimilarity
                                CenterSimilarity = m.GeometricSimilarity, // Map GeometricSimilarity to CenterSimilarity
                                OverallSimilarity = m.Confidence
                            }).ToList()
                        };
                    }
                }

                return new UnifiedTokenIdentificationResult
                {
                    Success = false,
                    Message = "No matching token found",
                    Confidence = result.Confidence,
                    TokenId = null,
                    TokenName = null,
                    SystemUsed = "unified"
                };
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
        /// Calculate geometric data from touch points
        /// </summary>
        private async Task<GeometricData> CalculateGeometricDataAsync(double[][] touchPoints)
        {
            // This is a simplified calculation - you can enhance this based on your needs
            var distances = new List<double>();
            var angles = new List<double>();
            
            // Calculate distances between consecutive points
            for (int i = 0; i < touchPoints.Length - 1; i++)
            {
                var distance = Math.Sqrt(
                    Math.Pow(touchPoints[i + 1][0] - touchPoints[i][0], 2) +
                    Math.Pow(touchPoints[i + 1][1] - touchPoints[i][1], 2)
                );
                distances.Add(distance);
            }
            
            // Calculate angles between consecutive line segments
            for (int i = 0; i < touchPoints.Length - 2; i++)
            {
                var angle = CalculateAngle(
                    touchPoints[i], touchPoints[i + 1], touchPoints[i + 2]
                );
                angles.Add(angle);
            }
            
            // Calculate center point
            var centerX = touchPoints.Average(p => p[0]);
            var centerY = touchPoints.Average(p => p[1]);
            var center = new CenterPoint { X = centerX, Y = centerY };
            
            return new GeometricData
            {
                Distances = distances.ToArray(),
                Angles = angles.ToArray(),
                Center = center,
                TouchCount = touchPoints.Length
            };
        }

        /// <summary>
        /// Calculate angle between three points
        /// </summary>
        private double CalculateAngle(double[] p1, double[] p2, double[] p3)
        {
            var v1 = new double[] { p2[0] - p1[0], p2[1] - p1[1] };
            var v2 = new double[] { p3[0] - p2[0], p3[1] - p2[1] };
            
            var dot = v1[0] * v2[0] + v1[1] * v2[1];
            var mag1 = Math.Sqrt(v1[0] * v1[0] + v1[1] * v1[1]);
            var mag2 = Math.Sqrt(v2[0] * v2[0] + v2[1] * v2[1]);
            
            if (mag1 == 0 || mag2 == 0) return 0;
            
            var cosAngle = dot / (mag1 * mag2);
            cosAngle = Math.Max(-1, Math.Min(1, cosAngle)); // Clamp to valid range
            
            return Math.Acos(cosAngle) * 180 / Math.PI; // Convert to degrees
        }

        /// <summary>
        /// Convert geometric data to TokenSignature
        /// </summary>
        private TokenSignature ConvertToTokenSignature(GeometricData geometricData)
        {
            return new TokenSignature
            {
                TouchCount = geometricData.TouchCount,
                Distances = System.Text.Json.JsonSerializer.Serialize(geometricData.Distances),
                Angles = System.Text.Json.JsonSerializer.Serialize(geometricData.Angles),
                Center = System.Text.Json.JsonSerializer.Serialize(new double[] { geometricData.Center.X, geometricData.Center.Y }),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
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
        /// UNIFIED SAVE FUNCTION - The single point for all token saving operations
        /// Handles both creation and updates across both systems
        /// Automatically associates tokens with the current user's team
        /// </summary>
        public async Task<UnifiedSaveResult> SaveTokenAsync(UnifiedTokenSaveRequest request)
        {
            try
            {
                var teamId = GetCurrentTeamId();
                var userId = GetCurrentUserId();
                
                _logger.LogInformation("Starting unified token save operation for token: {TokenName} in team: {TeamId}", 
                    request.Name, teamId);

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

                // Add team context to request
                request.TeamId = teamId;
                request.CreatedByUserId = userId;

                // Use unified system for all tokens
                return await SaveToUnifiedSystem(request);
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
        /// Save token to unified system
        /// </summary>
        private async Task<UnifiedSaveResult> SaveToUnifiedSystem(UnifiedTokenSaveRequest request)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    Token token;
                    TokenSignature signature;

                    if (request.TokenId.HasValue)
                    {
                        // Update existing token
                        token = await _context.Tokens
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
                            TrainingConsistency = 0,
                            TeamId = request.TeamId,
                            CreatedByUserId = request.CreatedByUserId,
                            CreatedByUserName = GetCurrentUserName(),
                            TokenGroupId = request.TokenGroupId
                        };

                        signature = new TokenSignature
                        {
                            TokenId = tokenId,
                            Token = token
                        };

                        token.Signature = signature;
                        _context.Tokens.Add(token);
                    }

                    // Update signature with geometric data
                    if (request.TouchPoints != null && request.TouchPoints.Length >= 2)
                    {
                        var geometricData = await CalculateGeometricDataAsync(request.TouchPoints);
                        
                        signature.TouchCount = request.TouchPoints.Length;
                        signature.Distances = JsonSerializer.Serialize(geometricData.Distances);
                        signature.Angles = JsonSerializer.Serialize(geometricData.Angles);
                        signature.Center = JsonSerializer.Serialize(new double[] { geometricData.Center.X, geometricData.Center.Y });

                        if (token.Signature == null)
                        {
                            _context.TokenSignatures.Add(signature);
                        }
                    }

                    await _context.SaveChangesAsync();
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

            // For new tokens, TokenGroupId is required
            if (!request.TokenId.HasValue && !request.TokenGroupId.HasValue)
            {
                return (false, "Token group must be specified for new tokens");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Determine which system to use for saving
        /// </summary>
        private async Task<string> DetermineSystemToUse(UnifiedTokenSaveRequest request)
        {
            // Always use unified system
            return "unified";
        }

        /// <summary>
        /// Generate a unique token ID
        /// </summary>
        private long GenerateTokenId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Get team's token list for dropdowns
        /// Returns tokens from groups bound to the current user's team in active game sessions
        /// </summary>
        public async Task<List<GroupedTeamTokenInfo>> GetTeamTokensAsync()
        {
            try
            {
                var teamId = GetCurrentTeamId();
                var groupedTokens = new List<GroupedTeamTokenInfo>();

                // Get active game sessions
                var activeSessions = await _context.GameSessions
                    .Where(s => s.Status == "Active")
                    .ToListAsync();

                if (!activeSessions.Any())
                {
                    return groupedTokens; // No active game sessions
                }

                // Get token bindings for this team in active sessions
                var activeSessionIds = activeSessions.Select(s => s.Id).ToList();
                var teamBindings = await _context.TokenBindings
                    .Where(b => b.TeamId == teamId && activeSessionIds.Contains(b.GameSessionId) && b.IsActive)
                    .ToListAsync();

                if (!teamBindings.Any())
                {
                    return groupedTokens; // No bindings for this team
                }

                // Get token groups for the bindings
                var boundGroupIds = teamBindings.Select(b => b.TokenGroupId).ToList();
                var tokenGroups = await _context.TokenGroups
                    .Where(g => boundGroupIds.Contains(g.Id) && g.IsActive)
                    .OrderBy(g => g.Name)
                    .ToListAsync();

                foreach (var group in tokenGroups)
                {
                    var binding = teamBindings.First(b => b.TokenGroupId == group.Id);
                    var groupTokens = new List<TeamTokenInfo>();

                    // Get all tokens for this group (both simplified and complex are now in same table)
                    var allTokens = await _context.Tokens
                        .Include(t => t.Signature)
                        .Where(t => t.TokenGroupId == group.Id && t.IsActive)
                        .OrderBy(t => t.Name)
                        .ToListAsync();

                    groupTokens.AddRange(allTokens.Select(t => new TeamTokenInfo
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Category = t.Category,
                        TouchCount = t.Signature?.TouchCount ?? 0,
                        System = "unified", // Now using unified system
                        CreatedAt = t.CreatedAt,
                        UsageCount = t.UsageCount
                    }));

                    if (groupTokens.Any())
                    {
                        groupedTokens.Add(new GroupedTeamTokenInfo
                        {
                            GroupId = group.Id,
                            GroupName = group.Name,
                            GroupCode = group.GroupCode,
                            GroupCategory = group.Category,
                            EntityName = binding.EntityName,
                            EntityCode = binding.EntityCode,
                            Tokens = groupTokens.OrderBy(t => t.Name).ToList()
                        });
                    }
                }

                return groupedTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team tokens");
                return new List<GroupedTeamTokenInfo>();
            }
        }

        /// <summary>
        /// UNIFIED DELETE FUNCTION - The single point for all token deletion operations
        /// Handles token deletion and cleanup across both systems
        /// Automatically handles team context and related data cleanup
        /// </summary>
        public async Task<UnifiedDeleteResult> DeleteTokenAsync(long tokenId)
        {
            try
            {
                var teamId = GetCurrentTeamId();
                var userId = GetCurrentUserId();
                
                _logger.LogInformation("Starting unified token delete operation for token: {TokenId} in team: {TeamId}", 
                    tokenId, teamId);

                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Find the token and verify it belongs to the current team
                    var token = await _context.Tokens
                        .Include(t => t.Signature)
                        .FirstOrDefaultAsync(t => t.Id == tokenId && t.TeamId == teamId);

                    if (token == null)
                    {
                        return new UnifiedDeleteResult
                        {
                            Success = false,
                            Message = "Token not found or access denied",
                            TokenId = tokenId
                        };
                    }

                    // Delete associated map markers first
                    var markers = await _context.MapMarkers
                        .Where(m => m.TokenId == tokenId)
                        .ToListAsync();
                    
                    if (markers.Any())
                    {
                        _context.MapMarkers.RemoveRange(markers);
                        _logger.LogInformation("Deleted {Count} map markers for token {TokenId}", markers.Count, tokenId);
                    }

                    // Delete token signature and related data
                    if (token.Signature != null)
                    {
                        // Delete related signature data
                        if (token.Signature.Stability != null)
                            _context.StabilityInfo.Remove(token.Signature.Stability);
                        
                        if (token.Signature.TouchProperties != null)
                            _context.TouchGeometry.Remove(token.Signature.TouchProperties);
                        
                        if (token.Signature.TouchPattern != null)
                            _context.TouchPatterns.Remove(token.Signature.TouchPattern);
                        
                        if (token.Signature.MultiTouchGeometry != null)
                            _context.MultiTouchGeometry.Remove(token.Signature.MultiTouchGeometry);

                        // Delete the signature itself
                        _context.TokenSignatures.Remove(token.Signature);
                    }

                    // Delete the token
                    _context.Tokens.Remove(token);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully deleted token {TokenId} and all related data", tokenId);

                    return new UnifiedDeleteResult
                    {
                        Success = true,
                        Message = $"Token '{token.Name}' deleted successfully",
                        TokenId = tokenId,
                        DeletedMarkersCount = markers.Count
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during unified token delete operation for token {TokenId}", tokenId);
                return new UnifiedDeleteResult
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
        
        // Additional properties for compatibility
        public long? TokenId { get; set; }
        public string? TokenName { get; set; }
        public List<UnifiedTokenMatchDetail>? MatchDetails { get; set; }
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
    /// Unified token save request - Single request format for all save operations
    /// Team context is automatically added by the DAL
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

        // Team context - automatically set by DAL
        internal string TeamId { get; set; } = string.Empty;
        internal string CreatedByUserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Token group ID - must be provided for new tokens
        /// </summary>
        public int? TokenGroupId { get; set; }
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

    /// <summary>
    /// Unified delete result
    /// </summary>
    public class UnifiedDeleteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long TokenId { get; set; }
        public int DeletedMarkersCount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Team token information for dropdowns
    /// </summary>
    public class TeamTokenInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public int TouchCount { get; set; }
        public string System { get; set; } = string.Empty; // "simplified" or "complex"
        public DateTime CreatedAt { get; set; }
        public int UsageCount { get; set; }
    }

    /// <summary>
    /// Grouped team token information for organized dropdowns
    /// </summary>
    public class GroupedTeamTokenInfo
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public string? GroupCategory { get; set; }
        public string? EntityName { get; set; } // e.g., "Company A", "Brigade 1"
        public string? EntityCode { get; set; } // e.g., "COMP_A", "BRIG_1"
        public List<TeamTokenInfo> Tokens { get; set; } = new();
    }

    public class UnifiedTokenMatchDetail
    {
        public long TokenId { get; set; }
        public string TokenName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double DistanceSimilarity { get; set; }
        public double AngleSimilarity { get; set; }
        public double CenterSimilarity { get; set; }
        public double OverallSimilarity { get; set; }
    }
}
