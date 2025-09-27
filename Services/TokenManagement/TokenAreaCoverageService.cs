using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;
using System.Text.Json;

namespace TechWebSol.Services.TokenManagement
{
    public interface ITokenAreaCoverageService
    {
        Task<TokenAreaCoverageResult> CreateInitialCoverageAsync(Guid tokenId, decimal latitude, decimal longitude, decimal radiusKm);
        Task<TokenAreaCoverageResult> UpdateCoverageAreaAsync(Guid tokenId, decimal latitude, decimal longitude, decimal radiusKm);
        Task<TokenAreaCoverageResult> RemoveCoverageAreasAsync(Guid tokenId);
        Task<TokenAreaCoverageResult> GetCoverageAreasAsync(Guid tokenId);
    }

    public class TokenAreaCoverageResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TokenAreaCoverage>? AreaCoverages { get; set; }
    }

    public class TokenAreaCoverageService : ITokenAreaCoverageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TokenAreaCoverageService> _logger;

        public TokenAreaCoverageService(
            ApplicationDbContext context,
            ILogger<TokenAreaCoverageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TokenAreaCoverageResult> CreateInitialCoverageAsync(Guid tokenId, decimal latitude, decimal longitude, decimal radiusKm)
        {
            try
            {
                // Check if coverage already exists
                var existingCoverage = await _context.TokenAreaCoverages
                    .FirstOrDefaultAsync(tac => tac.TokenId == tokenId && tac.CoverageType == "Operational" && tac.IsActive);

                if (existingCoverage != null)
                {
                    return new TokenAreaCoverageResult
                    {
                        Success = false,
                        Message = "Coverage area already exists for this token"
                    };
                }

                // Create circle geometry
                var geometry = CreateCircleGeometry(latitude, longitude, radiusKm);
                var areaKm2 = (decimal)(Math.PI * Math.Pow((double)radiusKm, 2));

                var areaCoverage = new TokenAreaCoverage
                {
                    TokenId = tokenId,
                    Name = "Operational Area",
                    Geometry = JsonSerializer.Serialize(geometry),
                    AreaKm2 = areaKm2,
                    RadiusKm = radiusKm,
                    CoverageType = "Operational",
                    ShapeType = "Circle",
                    IsActive = true,
                    IsDynamic = true,
                    Description = "Initial operational area for token",
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                _context.TokenAreaCoverages.Add(areaCoverage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created initial coverage area for token {TokenId} with radius {RadiusKm}km", 
                    tokenId, radiusKm);

                return new TokenAreaCoverageResult
                {
                    Success = true,
                    Message = "Coverage area created successfully",
                    AreaCoverages = new List<TokenAreaCoverage> { areaCoverage }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating initial coverage area for token {TokenId}", tokenId);
                return new TokenAreaCoverageResult
                {
                    Success = false,
                    Message = "Error creating coverage area"
                };
            }
        }

        public async Task<TokenAreaCoverageResult> UpdateCoverageAreaAsync(Guid tokenId, decimal latitude, decimal longitude, decimal radiusKm)
        {
            try
            {
                // Find existing dynamic coverage areas
                var existingCoverages = await _context.TokenAreaCoverages
                    .Where(tac => tac.TokenId == tokenId && tac.IsDynamic && tac.IsActive)
                    .ToListAsync();

                var updatedCoverages = new List<TokenAreaCoverage>();

                // Update or create default operational area
                var operationalArea = existingCoverages.FirstOrDefault(c => c.CoverageType == "Operational");
                
                if (operationalArea != null)
                {
                    // Update existing area
                    var geometry = CreateCircleGeometry(latitude, longitude, radiusKm);
                    operationalArea.Geometry = JsonSerializer.Serialize(geometry);
                    operationalArea.AreaKm2 = (decimal)(Math.PI * Math.Pow((double)radiusKm, 2));
                    operationalArea.RadiusKm = radiusKm;
                    operationalArea.LastUpdated = DateTime.UtcNow;
                    updatedCoverages.Add(operationalArea);
                }
                else
                {
                    // Create new coverage area
                    var createResult = await CreateInitialCoverageAsync(tokenId, latitude, longitude, radiusKm);
                    if (createResult.Success && createResult.AreaCoverages != null)
                    {
                        updatedCoverages.AddRange(createResult.AreaCoverages);
                    }
                }

                // Update other dynamic coverage areas if they exist
                foreach (var coverage in existingCoverages.Where(c => c.CoverageType != "Operational"))
                {
                    if (coverage.ShapeType == "Circle" && coverage.RadiusKm.HasValue)
                    {
                        var geometry = CreateCircleGeometry(latitude, longitude, coverage.RadiusKm.Value);
                        coverage.Geometry = JsonSerializer.Serialize(geometry);
                        coverage.AreaKm2 = (decimal)(Math.PI * Math.Pow((double)coverage.RadiusKm.Value, 2));
                        coverage.LastUpdated = DateTime.UtcNow;
                        updatedCoverages.Add(coverage);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated coverage areas for token {TokenId} at {Latitude}, {Longitude}", 
                    tokenId, latitude, longitude);

                return new TokenAreaCoverageResult
                {
                    Success = true,
                    Message = "Coverage areas updated successfully",
                    AreaCoverages = updatedCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coverage areas for token {TokenId}", tokenId);
                return new TokenAreaCoverageResult
                {
                    Success = false,
                    Message = "Error updating coverage areas"
                };
            }
        }

        public async Task<TokenAreaCoverageResult> RemoveCoverageAreasAsync(Guid tokenId)
        {
            try
            {
                var areaCoverages = await _context.TokenAreaCoverages
                    .Where(tac => tac.TokenId == tokenId && tac.IsActive)
                    .ToListAsync();

                foreach (var coverage in areaCoverages)
                {
                    coverage.IsActive = false;
                    coverage.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed coverage areas for token {TokenId}", tokenId);

                return new TokenAreaCoverageResult
                {
                    Success = true,
                    Message = "Coverage areas removed successfully",
                    AreaCoverages = areaCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing coverage areas for token {TokenId}", tokenId);
                return new TokenAreaCoverageResult
                {
                    Success = false,
                    Message = "Error removing coverage areas"
                };
            }
        }

        public async Task<TokenAreaCoverageResult> GetCoverageAreasAsync(Guid tokenId)
        {
            try
            {
                var areaCoverages = await _context.TokenAreaCoverages
                    .Where(tac => tac.TokenId == tokenId && tac.IsActive)
                    .ToListAsync();

                return new TokenAreaCoverageResult
                {
                    Success = true,
                    Message = "Coverage areas retrieved successfully",
                    AreaCoverages = areaCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coverage areas for token {TokenId}", tokenId);
                return new TokenAreaCoverageResult
                {
                    Success = false,
                    Message = "Error retrieving coverage areas"
                };
            }
        }

        /// <summary>
        /// Create circle geometry for coverage area
        /// </summary>
        private object CreateCircleGeometry(decimal lat, decimal lng, decimal radiusKm)
        {
            var radiusInDegrees = radiusKm / 111.32m; // Approximate conversion from km to degrees
            var coordinates = GenerateCircleCoordinates(lat, lng, radiusInDegrees);

            return new
            {
                type = "Polygon",
                coordinates = new[] { coordinates }
            };
        }

        /// <summary>
        /// Generate coordinates for a circle
        /// </summary>
        private decimal[][] GenerateCircleCoordinates(decimal centerLat, decimal centerLng, decimal radiusInDegrees)
        {
            var points = new List<decimal[]>();
            var numPoints = 32; // Number of points to create a smooth circle

            for (int i = 0; i <= numPoints; i++)
            {
                var angle = (2 * Math.PI * i) / numPoints;
                var lat = centerLat + (decimal)((double)radiusInDegrees * Math.Cos(angle));
                var lng = centerLng + (decimal)((double)radiusInDegrees * Math.Sin(angle));
                points.Add(new decimal[] { lng, lat }); // GeoJSON format: [longitude, latitude]
            }

            return points.ToArray();
        }
    }
}
