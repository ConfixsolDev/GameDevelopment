using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;
using TechWebSol.ViewModels;
using System.Globalization;

namespace TechWebSol.Services.TokenManagement
{
    public interface ITokenPlacementService
    {
        Task<TokenPlacementResult> PlaceTokenOnMapAsync(Guid tokenId, decimal latitude, decimal longitude, string userId);
        Task<TokenPlacementResult> UpdateTokenPositionAsync(Guid tokenId, decimal latitude, decimal longitude, string userId);
        Task<TokenPlacementResult> RemoveTokenFromMapAsync(Guid tokenId, string userId);
        Task<TokenPlacementResult> GetTokenPlacementInfoAsync(Guid tokenId);
    }

    public class TokenPlacementResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Token? Token { get; set; }
        public MapMarker? MapMarker { get; set; }
        public List<TokenAreaCoverage>? AreaCoverages { get; set; }
    }

    public class TokenPlacementService : ITokenPlacementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenAreaCoverageService _coverageService;
        private readonly ILogger<TokenPlacementService> _logger;
        private readonly ApplicationUserVM applicatonUser;


        public TokenPlacementService(
            ApplicationDbContext context,
            ITokenAreaCoverageService coverageService
             , IUserSessionService IUserSessionService,
            ILogger<TokenPlacementService> logger)
        {
            _context = context;
            _coverageService = coverageService;
            _logger = logger;
            applicatonUser = IUserSessionService.GetCurrentUser();
        }

        public async Task<TokenPlacementResult> PlaceTokenOnMapAsync(Guid tokenId, decimal latitude, decimal longitude, string userId)
        {
            try
            {
                // Get the token
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId);

                if (token == null)
                {
                    return new TokenPlacementResult
                    {
                        Success = false,
                        Message = "Token not found"
                    };
                }

                if (!token.IsActive)
                {
                    return new TokenPlacementResult
                    {
                        Success = false,
                        Message = "Token is not active"
                    };
                }

                // No longer using token's lat/long; MapMarkers hold placement state

                // Update token usage metadata only
                token.LastUsed = DateTime.UtcNow;
                token.UsageCount++;

                // Create map marker
                var mapMarker = new MapMarker
                {
                    TokenId = tokenId,
                    latitude = latitude.ToString(CultureInfo.InvariantCulture),
                    longitude = longitude.ToString(CultureInfo.InvariantCulture),
                    CreatedBy = userId,
                    IsActive = true,
                };

                _context.MapMarkers.Add(mapMarker);

                // Create area coverage if token has coverage radius
                List<TokenAreaCoverage>? areaCoverages = null;
                if (token.CoverageRadiusKm.HasValue && token.CoverageRadiusKm.Value > 0)
                {
                    var coverageResult = await _coverageService.CreateInitialCoverageAsync(
                        tokenId, latitude, longitude, token.CoverageRadiusKm.Value);
                    
                    if (coverageResult.Success)
                    {
                        areaCoverages = coverageResult.AreaCoverages;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Token {TokenId} placed successfully at {Latitude}, {Longitude}", 
                    tokenId, latitude, longitude);

                return new TokenPlacementResult
                {
                    Success = true,
                    Message = $"Token '{token.Name}' placed successfully",
                    Token = token,
                    MapMarker = mapMarker,
                    AreaCoverages = areaCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing token {TokenId} on map", tokenId);
                return new TokenPlacementResult
                {
                    Success = false,
                    Message = "Error placing token on map"
                };
            }
        }

        public async Task<TokenPlacementResult> UpdateTokenPositionAsync(Guid tokenId, decimal latitude, decimal longitude, string userId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId);

                if (token == null)
                {
                    return new TokenPlacementResult
                    {
                        Success = false,
                        Message = "Token not found"
                    };
                }

                // Update token usage metadata only
                token.LastUsed = DateTime.UtcNow;

                // Inactivate previous active marker and create a new one to preserve movement history
                var previousActiveMarker = await _context.MapMarkers.OrderByDescending(x=>x.CreatedDate)
                    .FirstOrDefaultAsync(m => m.TokenId == tokenId && m.IsActive);

                if (previousActiveMarker != null)
                {
                    previousActiveMarker.IsActive = false;
                }

                var mapMarker = new MapMarker
                {
                    TokenId = tokenId,
                    latitude = latitude.ToString(CultureInfo.InvariantCulture),
                    longitude = longitude.ToString(CultureInfo.InvariantCulture),
                    CreatedBy = userId,
                    IsActive = true,
                };
                _context.MapMarkers.Add(mapMarker);

                // Update area coverage if token has coverage radius
                List<TokenAreaCoverage>? areaCoverages = null;
                if (token.CoverageRadiusKm.HasValue && token.CoverageRadiusKm.Value > 0)
                {
                    var coverageResult = await _coverageService.UpdateCoverageAreaAsync(
                        tokenId, latitude, longitude, token.CoverageRadiusKm.Value);
                    
                    if (coverageResult.Success)
                    {
                        areaCoverages = coverageResult.AreaCoverages;
                    }
                }
                _context.Update(token);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Token {TokenId} position updated to {Latitude}, {Longitude}", 
                    tokenId, latitude, longitude);

                return new TokenPlacementResult
                {
                    Success = true,
                    Message = $"Token '{token.Name}' position updated successfully",
                    Token = token,
                    MapMarker = mapMarker,
                    AreaCoverages = areaCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token {TokenId} position", tokenId);
                return new TokenPlacementResult
                {
                    Success = false,
                    Message = "Error updating token position"
                };
            }
        }

        public async Task<TokenPlacementResult> RemoveTokenFromMapAsync(Guid tokenId, string userId)
        {
            try
            {
                var token = await _context.Tokens.FindAsync(tokenId);
                if (token == null)
                {
                    return new TokenPlacementResult
                    {
                        Success = false,
                        Message = "Token not found"
                    };
                }

                // Remove map marker
                var mapMarker = await _context.MapMarkers
                    .FirstOrDefaultAsync(m => m.TokenId == tokenId && m.IsActive);

                if (mapMarker != null)
                {
                    mapMarker.IsActive = false;
                }

                // Token no longer stores position; nothing to clear on token entity

                // Remove area coverages
                var areaCoverages = await _context.TokenAreaCoverages
                    .Where(tac => tac.TokenId == tokenId && tac.IsDynamic)
                    .ToListAsync();

                foreach (var coverage in areaCoverages)
                {
                    coverage.IsActive = false;
                    coverage.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Token {TokenId} removed from map", tokenId);

                return new TokenPlacementResult
                {
                    Success = true,
                    Message = $"Token '{token.Name}' removed from map",
                    Token = token,
                    MapMarker = mapMarker,
                    AreaCoverages = areaCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing token {TokenId} from map", tokenId);
                return new TokenPlacementResult
                {
                    Success = false,
                    Message = "Error removing token from map"
                };
            }
        }

        public async Task<TokenPlacementResult> GetTokenPlacementInfoAsync(Guid tokenId)
        {
            try
            {
                var token = await _context.Tokens
                    .Include(t => t.TokenGroup)
                    .FirstOrDefaultAsync(t => t.Id == tokenId);

                if (token == null)
                {
                    return new TokenPlacementResult
                    {
                        Success = false,
                        Message = "Token not found"
                    };
                }

                var mapMarker = await _context.MapMarkers.OrderByDescending(x=>x.CreatedDate)
                    .FirstOrDefaultAsync(m => m.TokenId == tokenId && m.IsActive);

                var areaCoverages = await _context.TokenAreaCoverages
                    .Where(tac => tac.TokenId == tokenId && tac.IsActive)
                    .ToListAsync();


                return new TokenPlacementResult
                {
                    Success = true,
                    Message = "Token placement info retrieved successfully",
                    Token = token,
                    MapMarker = mapMarker,
                    AreaCoverages = areaCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token {TokenId} placement info", tokenId);
                return new TokenPlacementResult
                {
                    Success = false,
                    Message = "Error retrieving token placement info"
                };
            }
        }
    }
}
