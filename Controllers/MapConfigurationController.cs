using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using TechWebSol.Data;
using TechWebSol.DTOs.Map;
using TechWebSol.Models.Map;

namespace TechWebSol.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        // Consistent JSON behavior for (de)serialization to/from raw geojson
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public MapController(ApplicationDbContext db) => _db = db;

        // POST: /api/map/saveAll
        // Creates a new MapDocument
        [HttpPost("saveAll")]
        public async Task<IActionResult> SaveAll([FromBody] MapSaveDto dto)
        {
            if (dto == null) return BadRequest("Body required.");

            var doc = new MapDocument
            {
                Name = string.IsNullOrWhiteSpace(dto.Name) ? "Default Map" : dto.Name.Trim(),
                RegionsJson = JsonSerializer.Serialize(dto.Regions ?? new { type = "FeatureCollection", features = Array.Empty<object>() }, JsonOpts),
                ObstaclesJson = JsonSerializer.Serialize(dto.Obstacles ?? new { type = "FeatureCollection", features = Array.Empty<object>() }, JsonOpts),
                SafeJson = JsonSerializer.Serialize(dto.Safe ?? new { type = "FeatureCollection", features = Array.Empty<object>() }, JsonOpts),
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            _db.MapDocuments.Add(doc);
            await _db.SaveChangesAsync();

            return Ok(new { id = doc.Id });
        }

        // PUT: /api/map/edit
        // Updates an existing MapDocument (partial updates supported)
        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] MapEditDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest("Valid id required.");

            var doc = await _db.MapDocuments.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (doc == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name))
                doc.Name = dto.Name.Trim();

            if (dto.Regions != null)
                doc.RegionsJson = JsonSerializer.Serialize(dto.Regions, JsonOpts);

            if (dto.Obstacles != null)
                doc.ObstaclesJson = JsonSerializer.Serialize(dto.Obstacles, JsonOpts);

            if (dto.Safe != null)
                doc.SafeJson = JsonSerializer.Serialize(dto.Safe, JsonOpts);

            doc.UpdatedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { id = doc.Id, updated = doc.UpdatedUtc });
        }

        // DELETE: /api/map/delete
        // Deletes a MapDocument by id (from body)
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] MapDeleteDto dto)
        {
            if (dto == null || dto.Id <= 0) return BadRequest("Valid id required.");

            var doc = await _db.MapDocuments.FindAsync(dto.Id);
            if (doc == null) return NotFound();

            _db.MapDocuments.Remove(doc);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // GET: /api/map/loadLatest
        // Returns the latest-updated document (or 404 if none)
        [HttpGet("loadLatest")]
        public async Task<ActionResult<MapLoadDto>> LoadLatest()
        {
            var latest = await _db.MapDocuments
                .OrderByDescending(x => x.UpdatedUtc)
                .FirstOrDefaultAsync();

            if (latest == null) return NotFound();

            return new MapLoadDto
            {
                Id = latest.Id,
                Name = latest.Name,
                Regions = JsonSerializer.Deserialize<object>(latest.RegionsJson, JsonOpts),
                Obstacles = JsonSerializer.Deserialize<object>(latest.ObstaclesJson, JsonOpts),
                Safe = JsonSerializer.Deserialize<object>(latest.SafeJson, JsonOpts)
            };
        }

        // GET: /api/map/load/{id}
        // Returns a specific document by id
        [HttpGet("load/{id:int}")]
        public async Task<ActionResult<MapLoadDto>> LoadById(int id)
        {
            var doc = await _db.MapDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            return new MapLoadDto
            {
                Id = doc.Id,
                Name = doc.Name,
                Regions = JsonSerializer.Deserialize<object>(doc.RegionsJson, JsonOpts),
                Obstacles = JsonSerializer.Deserialize<object>(doc.ObstaclesJson, JsonOpts),
                Safe = JsonSerializer.Deserialize<object>(doc.SafeJson, JsonOpts)
            };
        }

        // GET: /api/map/list?page=1&pageSize=20
        // Lightweight listing for UI (no big JSON payloads)
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var query = _db.MapDocuments.AsNoTracking().OrderByDescending(x => x.UpdatedUtc);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new { x.Id, x.Name, x.CreatedUtc, x.UpdatedUtc })
                .ToListAsync();

            return Ok(new { page, pageSize, total, items });
        }
    }
    public class MapConfigurationController : Controller
    {
        [HttpGet]
        public IActionResult ModePartial(string mode)
        {
            // normalize
            mode = (mode ?? "").ToLowerInvariant();

            // pick the partial and optional model per mode
            return mode switch
            {
                "3d" => PartialView("Partial/_3D" /*, modelFor3D*/),
                "4d" => PartialView("Partial/_4D" /*, modelFor4D*/),
                _ => BadRequest("Unknown mode")
            };
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
