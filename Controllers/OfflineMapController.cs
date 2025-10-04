using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWebSol.Services;

namespace TechWebSol.Controllers
{
    [AllowAnonymous]
    [Route("tiles")]
    public class OfflineMapController : Controller
    {
        private readonly IOfflineMapService offlineMapService;

        public OfflineMapController(IOfflineMapService offlineMapService)
        {
            this.offlineMapService = offlineMapService;
        }

        // GET /tiles/{z}/{x}/{y}.{ext}
        [HttpGet("{z:int}/{x:int}/{y:int}.{ext}")]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetTile(int z, int x, int y, string ext, CancellationToken ct)
        {
            var result = await offlineMapService.GetTileAsync(z, x, y, ct);
            if (result == null)
            {
                return NotFound();
            }

            // Ignore requested ext; serve according to stored format/content type
            Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            return File(result.Value.data, result.Value.contentType);
        }

        // GET /tiles/metadata
        [HttpGet("metadata")]
        public async Task<IActionResult> GetMetadata(CancellationToken ct)
        {
            var meta = await offlineMapService.GetMetadataAsync(ct);
            if (meta == null) return NotFound();
            return Json(new
            {
                bounds = meta.Bounds,
                center = meta.Center,
                zoom = meta.Zoom,
                minZoom = meta.MinZoom,
                maxZoom = meta.MaxZoom,
                name = meta.Name,
                format = meta.Format,
                attribution = meta.Attribution
            });
        }
    }
}


