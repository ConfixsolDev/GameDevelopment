using Microsoft.AspNetCore.Mvc;
using TechWebSol.Models.MapManagement;
using TechWebSol.Services.MapManagement;

namespace TechWebSol.Controllers.MapManagement
{
	[ApiController]
	[Route("")]
    public class JobsController : ControllerBase
    {
        private readonly JobStore _jobs;
        private readonly TileService _tiles;

        public JobsController(JobStore jobs, TileService tiles)
        {
            _jobs = jobs;
            _tiles = tiles;
        }

        [HttpGet("/jobs")]
        public IActionResult Jobs()
        {
            var list = _jobs.Jobs.Select(kvp => new
            {
                id = kvp.Key,
                progress = kvp.Value.Progress,
                total = kvp.Value.Total,
                done = kvp.Value.Done,
                error = kvp.Value.Error,
                format = kvp.Value.Format,
                style = kvp.Value.Style,
                createdUtc = kvp.Value.CreatedUtc,
                completedUtc = kvp.Value.CompletedUtc,
                size = kvp.Value.FileBytes?.Length ?? 0,
                fileName = kvp.Value.FileName
            }).OrderByDescending(x => x.createdUtc).ToList();
            return new JsonResult(list);
        }

        [HttpPost("/preview_tile_count")]
        public IActionResult PreviewTileCount([FromBody] PreviewRequest body)
        {
            if (body.Bounds is null || body.Zoom_Levels is null || body.Zoom_Levels.Count == 0)
            {
                return BadRequest(new { error = "Missing bounds or zoom levels" });
            }

            const int MAX_TILE_COUNT = 50000;
            long total = 0;
            foreach (var z in body.Zoom_Levels)
            {
                var n = 1 << z;
                var (x1, y1) = TileMath.Deg2Num(body.Bounds.North, body.Bounds.West, z);
                var (x2, y2) = TileMath.Deg2Num(body.Bounds.South, body.Bounds.East, z);
                var xMin = Math.Min(x1, x2);
                var xMax = Math.Max(x1, x2);
                var yMin = Math.Min(y1, y2);
                var yMax = Math.Max(y1, y2);

                // Clamp to valid tile coordinates
                xMin = Math.Max(0, Math.Min(xMin, n - 1));
                xMax = Math.Max(0, Math.Min(xMax, n - 1));
                yMin = Math.Max(0, Math.Min(yMin, n - 1));
                yMax = Math.Max(0, Math.Min(yMax, n - 1));

                total += (xMax - xMin + 1) * (yMax - yMin + 1);
                if (total > MAX_TILE_COUNT)
                {
                    return BadRequest(new { error = $"Too many tiles: {total}" });
                }
            }

            // Calculate geographic area
            var areaKm2 = TileMath.CalculateAreaKm2(body.Bounds.North, body.Bounds.South, body.Bounds.East, body.Bounds.West);
            var areaMi2 = TileMath.CalculateAreaMi2(body.Bounds.North, body.Bounds.South, body.Bounds.East, body.Bounds.West);

            return new JsonResult(new
            {
                tile_count = total,
                area_km2 = Math.Round(areaKm2, 2),
                area_mi2 = Math.Round(areaMi2, 2)
            });
        }

        [HttpPost("/download_tiles")]
        public IActionResult DownloadTiles([FromBody] DownloadRequest body)
        {
            if (body == null) return BadRequest("Missing request body.");
            var job = new DownloadJob
            {
                Progress = 0,
                Total = 1,
                Done = false,
                Error = null,
                FileBytes = null,
                Format = string.IsNullOrWhiteSpace(body.Format) ? "zip" : body.Format.ToLowerInvariant(),
                Style = string.IsNullOrWhiteSpace(body.Map_Style) ? "map" : body.Map_Style.ToLowerInvariant(),
                CreatedUtc = DateTime.UtcNow
            };
            var jobId = _jobs.Add(job);

            _ = Task.Run(async () =>
            {
                try
                {
                    var set = new HashSet<(int z, int x, int y)>();
                    // Remove TILE_MARGIN to ensure bounds match exactly what user selected
                    foreach (var z in body.Zoom_Levels)
                    {
                        var n = 1 << z;
                        var (x1, y1) = TileMath.Deg2Num(body.Bounds.North, body.Bounds.West, z);
                        var (x2, y2) = TileMath.Deg2Num(body.Bounds.South, body.Bounds.East, z);
                        var xMin = Math.Min(x1, x2);
                        var xMax = Math.Max(x1, x2);
                        var yMin = Math.Min(y1, y2);
                        var yMax = Math.Max(y1, y2);

                        // Clamp to valid tile coordinates without wrapping
                        xMin = Math.Max(0, Math.Min(xMin, n - 1));
                        xMax = Math.Max(0, Math.Min(xMax, n - 1));
                        yMin = Math.Max(0, Math.Min(yMin, n - 1));
                        yMax = Math.Max(0, Math.Min(yMax, n - 1));

                        for (var x = xMin; x <= xMax; x++)
                        {
                            for (var y = yMin; y <= yMax; y++)
                            {
                                set.Add((z, x, y));
                            }
                        }
                    }
                    var tiles = set.ToList();
                    job.Total = tiles.Count;

                    byte[]? resultBytes = null;
                    if (job.Format == "mbtiles")
                    {
                        // Pass original bounds to ensure accurate metadata
                        var originalBounds = (body.Bounds.North, body.Bounds.South, body.Bounds.East, body.Bounds.West);
                        var mbtilesResult = await _tiles.CreateMbtilesAsync(tiles, jobId, job.Style, p => job.Progress = p, originalBounds);
                        if (mbtilesResult != null)
                        {
                            resultBytes = mbtilesResult.Bytes;
                            job.FileName = mbtilesResult.FileName;
                        }
                    }
                    else
                    {
                        resultBytes = await _tiles.CreateZipAsync(tiles, jobId, job.Style, p => job.Progress = p);
                    }

                    job.FileBytes = resultBytes;
                    job.Done = resultBytes != null;
                    if (resultBytes == null) job.Error = "Failed to create file";
                    job.CompletedUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    job.Error = $"Download failed: {ex.Message}";
                    job.CompletedUtc = DateTime.UtcNow;
                }
            });

            return new JsonResult(new { job_id = jobId });
        }

        [HttpGet("/progress/{jobId}")]
        public async Task Progress(string jobId)
        {
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";
            Response.Headers.TryAdd("X-Accel-Buffering", "no");
            Response.ContentType = "text/event-stream";

            while (true)
            {
                if (!_jobs.TryGet(jobId, out var job))
                {
                    await Response.WriteAsync("data: error\n\n");
                    await Response.Body.FlushAsync();
                    break;
                }

                if (job.Error is not null)
                {
                    await Response.WriteAsync("data: error\n\n");
                    await Response.Body.FlushAsync();
                    break;
                }

                if (job.Done && job.FileBytes is not null)
                {
                    await Response.WriteAsync("data: ready\n\n");
                    await Response.Body.FlushAsync();
                    break;
                }

                await Response.WriteAsync($"data: {job.Progress} / {job.Total}\n\n");
                await Response.Body.FlushAsync();
                await Task.Delay(500);
            }
        }

        [HttpGet("/get_file/{jobId}")]
        public IActionResult GetFile(string jobId)
        {
            if (!_jobs.TryGet(jobId, out var job))
            {
                return NotFound("Job not found");
            }
            if (job.FileBytes is null)
            {
                return NotFound("File not ready");
            }

            var stylePrefix = job.Style == "map" ? "map" : "satellite";
            var filename = $"{stylePrefix}_tiles.{job.Format}";
            return File(job.FileBytes, "application/octet-stream", filename);
        }

        [HttpGet("/validate_mbtiles/{jobId}")]
        public async Task<IActionResult> ValidateMbTiles(string jobId)
        {
            try
            {
                if (!_jobs.TryGet(jobId, out var job))
                {
                    return NotFound(new { error = "Job not found", job_id = jobId });
                }

                if (job.Format != "mbtiles")
                {
                    return BadRequest(new { error = $"Job format is '{job.Format}', not 'mbtiles'" });
                }

                if (job.FileBytes is null)
                {
                    return BadRequest(new { error = "File not ready yet", done = job.Done, progress = job.Progress, total = job.Total });
                }
                // Write to temp file for validation
                var tempPath = Path.Combine(Path.GetTempPath(), $"validate_{jobId}.mbtiles");
                await System.IO.File.WriteAllBytesAsync(tempPath, job.FileBytes);

                var cs = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                {
                    DataSource = tempPath,
                    Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly
                }.ToString();

                var fileSize = job.FileBytes.Length;
                long tileCount = 0;
                var metadata = new Dictionary<string, string>();
                var issues = new List<string>();

                await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(cs);
                await conn.OpenAsync();

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND (name='tiles' OR name='metadata')";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    var tables = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        tables.Add(reader.GetString(0));
                    }
                    if (!tables.Contains("tiles")) issues.Add("Missing 'tiles' table");
                    if (!tables.Contains("metadata")) issues.Add("Missing 'metadata' table");
                }

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM tiles";
                    tileCount = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
                    if (tileCount == 0) issues.Add("No tiles in database");
                }

                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name, value FROM metadata";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var name = reader.GetString(0);
                        var value = reader.GetString(1);
                        metadata[name] = value;
                    }
                }

                var requiredFields = new[] { "name", "type", "version", "format", "bounds", "center", "minzoom", "maxzoom" };
                foreach (var field in requiredFields)
                {
                    if (!metadata.ContainsKey(field))
                    {
                        issues.Add($"Missing required metadata field: {field}");
                    }
                }

                await conn.CloseAsync();
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                try { System.IO.File.Delete(tempPath); } catch { }

                var result = new
                {
                    valid = true,
                    file_size = fileSize,
                    tile_count = tileCount,
                    metadata = metadata,
                    issues = issues,
                    maptiler_ready = issues.Count == 0,
                    message = issues.Count == 0 ? "File is valid and ready for upload to MapTiler" : "File has issues that may prevent upload"
                };

                return new JsonResult(result);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                return StatusCode(500, new { valid = false, error = ex.Message, error_type = ex.GetType().Name, stack_trace = ex.StackTrace, maptiler_ready = false, message = "File validation failed - file may be corrupted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { valid = false, error = ex.Message, error_type = ex.GetType().Name, maptiler_ready = false, message = "Validation endpoint error" });
            }
        }
    }
}


