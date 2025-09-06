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
    public class MapMarkerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MapMarkerController> _logger;

        public MapMarkerController(ApplicationDbContext context, ILogger<MapMarkerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all map markers
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MapMarker>>> GetMapMarkers()
        {
            try
            {
                var markers = await _context.MapMarkers
                    .Include(m => m.Token)
                    .ToListAsync();
                return Ok(markers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map markers");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get map markers by token ID
        /// </summary>
        [HttpGet("by-token/{tokenId}")]
        public async Task<ActionResult<IEnumerable<MapMarker>>> GetMapMarkersByToken(long tokenId)
        {
            try
            {
                var markers = await _context.MapMarkers
                    .Where(m => m.TokenId == tokenId)
                    .Include(m => m.Token)
                    .ToListAsync();
                return Ok(markers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map markers for token {TokenId}", tokenId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get map marker by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MapMarker>> GetMapMarker(string id)
        {
            try
            {
                var marker = await _context.MapMarkers
                    .Include(m => m.Token)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (marker == null)
                    return NotFound();

                return Ok(marker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving map marker {MarkerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new map marker
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MapMarker>> CreateMapMarker([FromBody] MapMarker marker)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(marker.Id))
                    return BadRequest("Marker ID is required");

                if (string.IsNullOrWhiteSpace(marker.Location))
                    return BadRequest("Location data is required");

                if (marker.TokenId <= 0)
                    return BadRequest("Valid Token ID is required");

                // Check if token exists
                var token = await _context.Tokens.FindAsync(marker.TokenId);
                if (token == null)
                    return BadRequest("Token not found");

                // Check if marker already exists
                var existingMarker = await _context.MapMarkers.FindAsync(marker.Id);
                if (existingMarker != null)
                    return BadRequest("Marker with this ID already exists");

                marker.CreatedAt = DateTime.UtcNow;
                marker.TokenName = token.Name;

                _context.MapMarkers.Add(marker);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMapMarker), new { id = marker.Id }, marker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating map marker");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing map marker
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMapMarker(string id, [FromBody] MapMarker marker)
        {
            try
            {
                if (id != marker.Id)
                    return BadRequest("Marker ID mismatch");

                var existingMarker = await _context.MapMarkers.FindAsync(id);
                if (existingMarker == null)
                    return NotFound();

                existingMarker.Location = marker.Location;
                existingMarker.TokenName = marker.TokenName;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating map marker {MarkerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a map marker
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMapMarker(string id)
        {
            try
            {
                var marker = await _context.MapMarkers.FindAsync(id);
                if (marker == null)
                    return NotFound();

                _context.MapMarkers.Remove(marker);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting map marker {MarkerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete all map markers for a specific token
        /// </summary>
        [HttpDelete("by-token/{tokenId}")]
        public async Task<IActionResult> DeleteMapMarkersByToken(long tokenId)
        {
            try
            {
                var markers = await _context.MapMarkers
                    .Where(m => m.TokenId == tokenId)
                    .ToListAsync();

                _context.MapMarkers.RemoveRange(markers);
                await _context.SaveChangesAsync();

                return Ok(new { deletedCount = markers.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting map markers for token {TokenId}", tokenId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Bulk create map markers
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult> BulkCreateMapMarkers([FromBody] List<MapMarker> markers)
        {
            if (markers == null || markers.Count == 0)
                return BadRequest("No markers provided.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var results = new List<object>();
                var executionStrategy = _context.Database.CreateExecutionStrategy();

                // Run the whole operation under the execution strategy so it can retry safely.
                return await executionStrategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    // Batch fetch to reduce per-item queries
                    var markerIds = markers.Select(m => m.Id).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                    var tokenIds = markers.Select(m => m.TokenId).Where(id => id > 0).Distinct().ToList();

                    // Replace this line:
                    // var existingMarkerIds = await _context.MapMarkers
                    //     .Where(m => markerIds.Contains(m.Id))
                    //     .Select(m => m.Id)
                    //     .ToHashSetAsync();

                    // With this:
                    var existingMarkerIds = (await _context.MapMarkers
                        .Where(m => markerIds.Contains(m.Id))
                        .Select(m => m.Id)
                        .ToListAsync())
                        .ToHashSet();

                    var tokensById = await _context.Tokens
                        .Where(t => tokenIds.Contains(t.Id))
                        .ToDictionaryAsync(t => t.Id);

                    foreach (var marker in markers)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(marker.Id))
                            {
                                results.Add(new { marker.Id, Success = false, Error = "Marker ID is required" });
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(marker.Location))
                            {
                                results.Add(new { marker.Id, Success = false, Error = "Location data is required" });
                                continue;
                            }

                            if (marker.TokenId <= 0)
                            {
                                results.Add(new { marker.Id, Success = false, Error = "Valid Token ID is required" });
                                continue;
                            }

                            if (!tokensById.TryGetValue(marker.TokenId, out var token))
                            {
                                results.Add(new { marker.Id, Success = false, Error = "Token not found" });
                                continue;
                            }

                            if (existingMarkerIds.Contains(marker.Id))
                            {
                                _context.MapMarkers.Update(marker);
                                continue;
                            }

                            marker.CreatedAt = DateTime.UtcNow;
                            marker.TokenName = token.Name;

                            _context.MapMarkers.Add(marker);
                            existingMarkerIds.Add(marker.Id); // prevent duplicates within the same payload
                            results.Add(new { marker.Id, Success = true });
                        }
                        catch (Exception ex)
                        {
                            results.Add(new { marker.Id, Success = false, Error = ex.Message });
                        }
                    }

                    // Save first, THEN commit.
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(results);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating map markers");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}

