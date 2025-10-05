using Microsoft.EntityFrameworkCore;
using TechWebSol.Data;
using TechWebSol.Models;
using System.Text.Json;

namespace TechWebSol.Services.TokenManagement
{
    public interface ITokenAreaCoverageService
    {
        Task<TokenAreaCoverageResult> CreateInitialCoverageAsync(Guid tokenId, decimal latitude, decimal longitude, decimal frontRadiusKm, decimal rearRadiusKm, decimal? sideRadiusKm = null);
        Task<TokenAreaCoverageResult> UpdateCoverageAreaAsync(Guid tokenId, decimal latitude, decimal longitude, decimal frontRadiusKm, decimal rearRadiusKm, decimal? sideRadiusKm = null);
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

        public async Task<TokenAreaCoverageResult> CreateInitialCoverageAsync(Guid tokenId, decimal latitude, decimal longitude, decimal frontRadiusKm, decimal rearRadiusKm, decimal? sideRadiusKm = null)
        {
            try
            {
                // Get the token to use its coverage attributes
                var token = await _context.Tokens.FirstOrDefaultAsync(t => t.Id == tokenId);
                if (token == null)
                {
                    return new TokenAreaCoverageResult
                    {
                        Success = false,
                        Message = "Token not found"
                    };
                }

                // Use token's coverage attributes if available, otherwise use provided parameters
                var frontKm = token.FrontCoverageKm ?? frontRadiusKm;
                var rearKm = token.RearCoverageKm ?? rearRadiusKm;
                var sideKm = token.SideCoverageKm ?? sideRadiusKm ?? (frontKm + rearKm) / 2;

                // Check if coverage already exists
                var existingCoverage = await _context.TokenAreaCoverages
                    .FirstOrDefaultAsync(tac => tac.TokenId == tokenId && tac.CoverageType == "Frontside" && tac.IsActive);

                if (existingCoverage != null)
                {
                    return new TokenAreaCoverageResult
                    {
                        Success = false,
                        Message = "Coverage areas already exist for this token"
                    };
                }

                var areaCoverages = new List<TokenAreaCoverage>();

                // Create single 4-sided polygon coverage area using token attributes
                var geometry = Create4SidedPolygonGeometry(latitude, longitude, frontKm, rearKm, sideKm, 0);
                var areaKm2 = Calculate4SidedPolygonArea(frontKm, rearKm, sideKm);

                var coverage = new TokenAreaCoverage
                {
                    TokenId = tokenId,
                    Name = "Token Coverage",
                    Geometry = JsonSerializer.Serialize(geometry),
                    AreaKm2 = areaKm2,
                    FrontRadiusKm = frontKm,
                    RearRadiusKm = rearKm,
                    SideRadiusKm = sideKm,
                    RotationDegrees = 0,
                    CoverageType = "Operational",
                    ShapeType = "Oval",
                    IsActive = true,
                    IsDynamic = true,
                    Description = "Token coverage area based on front/rear/side radius",
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                areaCoverages.Add(coverage);

                _context.TokenAreaCoverages.AddRange(areaCoverages);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created initial coverage area for token {TokenId} using token attributes", tokenId);

                return new TokenAreaCoverageResult
                {
                    Success = true,
                    Message = "Coverage areas created successfully",
                    AreaCoverages = areaCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating initial coverage areas for token {TokenId}", tokenId);
                return new TokenAreaCoverageResult
                {
                    Success = false,
                    Message = "Error creating coverage areas"
                };
            }
        }

        public async Task<TokenAreaCoverageResult> UpdateCoverageAreaAsync(Guid tokenId, decimal latitude, decimal longitude, decimal frontRadiusKm, decimal rearRadiusKm, decimal? sideRadiusKm = null)
        {
            try
            {
                // Get the token to use its coverage attributes
                var token = await _context.Tokens.FirstOrDefaultAsync(t => t.Id == tokenId);
                if (token == null)
                {
                    return new TokenAreaCoverageResult
                    {
                        Success = false,
                        Message = "Token not found"
                    };
                }

                // Use token's coverage attributes if available, otherwise use provided parameters
                var frontKm = token.FrontCoverageKm ?? frontRadiusKm;
                var rearKm = token.RearCoverageKm ?? rearRadiusKm;
                var sideKm = token.SideCoverageKm ?? sideRadiusKm ?? (frontKm + rearKm) / 2;

                // Find existing dynamic coverage areas
                var existingCoverages = await _context.TokenAreaCoverages
                    .Where(tac => tac.TokenId == tokenId && tac.IsDynamic && tac.IsActive)
                    .ToListAsync();

                var updatedCoverages = new List<TokenAreaCoverage>();

                // Update or create single coverage area
                var existingCoverage = existingCoverages.FirstOrDefault(c => c.CoverageType == "Operational");
                if (existingCoverage != null)
                {
                    existingCoverage.Geometry = JsonSerializer.Serialize(Create4SidedPolygonGeometry(latitude, longitude, frontKm, rearKm, sideKm, 0));
                    existingCoverage.AreaKm2 = Calculate4SidedPolygonArea(frontKm, rearKm, sideKm);
                    existingCoverage.FrontRadiusKm = frontKm;
                    existingCoverage.RearRadiusKm = rearKm;
                    existingCoverage.SideRadiusKm = sideKm;
                    existingCoverage.RotationDegrees = 0;
                    existingCoverage.LastUpdated = DateTime.UtcNow;
                    updatedCoverages.Add(existingCoverage);
                }

                // If no existing coverage areas, create new ones
                if (!updatedCoverages.Any())
                {
                    var createResult = await CreateInitialCoverageAsync(tokenId, latitude, longitude, frontKm, rearKm, sideKm);
                    if (createResult.Success && createResult.AreaCoverages != null)
                    {
                        updatedCoverages.AddRange(createResult.AreaCoverages);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated coverage areas for token {TokenId} at {Latitude}, {Longitude} using token attributes", 
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
        /// Create 4-sided polygon geometry for coverage area
        /// </summary>
        private object Create4SidedPolygonGeometry(decimal lat, decimal lng, decimal frontKm, decimal rearKm, decimal sideKm, decimal rotationDegrees)
        {
            var frontRadiusInDegrees = frontKm / 111.32m; // Approximate conversion from km to degrees
            var rearRadiusInDegrees = rearKm / 111.32m;
            var sideRadiusInDegrees = sideKm / 111.32m;
            
            var coordinates = Generate4SidedPolygonCoordinates(lat, lng, frontRadiusInDegrees, rearRadiusInDegrees, sideRadiusInDegrees, rotationDegrees);

            return new
            {
                type = "Polygon",
                coordinates = new[] { coordinates }
            };
        }

        /// <summary>
        /// Create oval geometry for coverage area
        /// </summary>
        private object CreateOvalGeometry(decimal lat, decimal lng, decimal frontKm, decimal rearKm, decimal sideKm, decimal rotationDegrees)
        {
            var frontRadiusInDegrees = frontKm / 111.32m; // Approximate conversion from km to degrees
            var rearRadiusInDegrees = rearKm / 111.32m;
            var sideRadiusInDegrees = sideKm / 111.32m;
            
            var coordinates = GenerateOvalCoordinates(lat, lng, frontRadiusInDegrees, rearRadiusInDegrees, sideRadiusInDegrees, rotationDegrees);

            return new
            {
                type = "Polygon",
                coordinates = new[] { coordinates }
            };
        }

        /// <summary>
        /// Generate coordinates for a 4-sided polygon (diamond/rhombus shape)
        /// </summary>
        private decimal[][] Generate4SidedPolygonCoordinates(decimal centerLat, decimal centerLng, decimal frontRadiusDegrees, decimal rearRadiusDegrees, decimal sideRadiusDegrees, decimal rotationDegrees)
        {
            var points = new List<decimal[]>();
            
            // Create 4 points for diamond/rhombus shape
            var basePoints = new decimal[][]
            {
                new decimal[] { centerLng, centerLat + frontRadiusDegrees }, // Front (North)
                new decimal[] { centerLng + sideRadiusDegrees, centerLat },  // Right (East)
                new decimal[] { centerLng, centerLat - rearRadiusDegrees },   // Rear (South)
                new decimal[] { centerLng - sideRadiusDegrees, centerLat }    // Left (West)
            };
            
            // Apply rotation if needed
            if (rotationDegrees != 0)
            {
                var rotationRad = (double)rotationDegrees * Math.PI / 180.0;
                var cos = Math.Cos(rotationRad);
                var sin = Math.Sin(rotationRad);
                
                foreach (var point in basePoints)
                {
                    var x = (double)(point[0] - centerLng); // lng offset
                    var y = (double)(point[1] - centerLat); // lat offset
                    
                    // Apply rotation
                    var rotatedX = x * cos - y * sin;
                    var rotatedY = x * sin + y * cos;
                    
                    points.Add(new decimal[] { 
                        centerLng + (decimal)rotatedX, 
                        centerLat + (decimal)rotatedY 
                    });
                }
            }
            else
            {
                points.AddRange(basePoints);
            }
            
            // Close the polygon by adding the first point at the end
            points.Add(points[0]);
            
            return points.ToArray();
        }

        /// <summary>
        /// Generate coordinates for an oval shape
        /// </summary>
        private decimal[][] GenerateOvalCoordinates(decimal centerLat, decimal centerLng, decimal frontRadiusDegrees, decimal rearRadiusDegrees, decimal sideRadiusDegrees, decimal rotationDegrees)
        {
            var points = new List<decimal[]>();
            var numPoints = 64; // Increased points for smoother circles

            for (int i = 0; i <= numPoints; i++)
            {
                var angle = (2 * Math.PI * i) / numPoints;
                var rotatedAngle = angle + (double)rotationDegrees * Math.PI / 180.0;
                
                // Calculate radius based on angle for proper oval shape
                decimal radius;
                
                // Use parametric equation for ellipse: x = a*cos(t), y = b*sin(t)
                // where a is semi-major axis and b is semi-minor axis
                var cosAngle = Math.Cos(angle);
                var sinAngle = Math.Sin(angle);
                
                // Determine semi-major and semi-minor axes based on front/rear/side
                var semiMajorAxis = Math.Max((double)frontRadiusDegrees, (double)rearRadiusDegrees);
                var semiMinorAxis = (double)sideRadiusDegrees;
                
                // Calculate radius using ellipse formula
                var radiusX = semiMajorAxis * Math.Abs(cosAngle);
                var radiusY = semiMinorAxis * Math.Abs(sinAngle);
                radius = (decimal)Math.Sqrt(radiusX * radiusX + radiusY * radiusY);

                var lat = centerLat + (decimal)((double)radius * Math.Cos(rotatedAngle));
                var lng = centerLng + (decimal)((double)radius * Math.Sin(rotatedAngle));
                points.Add(new decimal[] { lng, lat }); // GeoJSON format: [longitude, latitude]
            }

            return points.ToArray();
        }

        /// <summary>
        /// Calculate area of a 4-sided polygon using the shoelace formula
        /// </summary>
        private decimal Calculate4SidedPolygonArea(decimal frontKm, decimal rearKm, decimal sideKm)
        {
            // For a diamond/rhombus shape, we can calculate area as:
            // Area = (diagonal1 * diagonal2) / 2
            // Where diagonal1 = front + rear, diagonal2 = side + side
            var diagonal1 = frontKm + rearKm;
            var diagonal2 = sideKm + sideKm; // Both sides are equal
            return (diagonal1 * diagonal2) / 2;
        }

        /// <summary>
        /// Calculate area of an oval using ellipse formula: π * a * b
        /// </summary>
        private decimal CalculateOvalArea(decimal frontKm, decimal rearKm, decimal sideKm)
        {
            var semiMajorAxis = Math.Max((double)frontKm, (double)rearKm) / 2;
            var semiMinorAxis = (double)sideKm / 2;
            return (decimal)(Math.PI * semiMajorAxis * semiMinorAxis);
        }
    }
}
