using Microsoft.AspNetCore.Mvc;
using TechWebSol.Models.MapManagement;
using TechWebSol.Services.MapManagement;
using System.Collections.Concurrent;

namespace TechWebSol.Controllers.MapManagement
{
	[ApiController]
	[Route("")]
	public class JobsController : ControllerBase
	{
	// Connection pool for MBTiles files to improve performance
	private static readonly ConcurrentDictionary<string, string> _connectionStrings = new();
	
	// Semaphore to limit concurrent tile requests per file (configurable limit)
	private const int MAX_CONCURRENT_TILE_REQUESTS = 100; // Increase to 200-300 if needed, decrease to 50 if issues
	private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileSemaphores = new();
	
	// Performance monitoring
	private static long _totalTileRequests = 0;
	private static long _failedTileRequests = 0;
		private readonly JobStore _jobs;
		private readonly TileService _tiles;
		private readonly TerrainDownloadService _terrain;

		public JobsController(JobStore jobs, TileService tiles, TerrainDownloadService terrain)
		{
			_jobs = jobs;
			_tiles = tiles;
			_terrain = terrain;
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
			fileName = kvp.Value.FileName,
			terrainDataFileName = kvp.Value.TerrainDataFileName,
			terrainDataError = kvp.Value.TerrainDataError
		}).OrderByDescending(x => x.createdUtc).ToList();
		return new JsonResult(list);
	}
	
	[HttpGet("/mbtiles/stats")]
	public IActionResult GetTileStats()
	{
		var total = Interlocked.Read(ref _totalTileRequests);
		var failed = Interlocked.Read(ref _failedTileRequests);
		var successRate = total > 0 ? ((total - failed) / (double)total * 100) : 100;
		
		return new JsonResult(new
		{
			maxConcurrentRequests = MAX_CONCURRENT_TILE_REQUESTS,
			totalRequests = total,
			failedRequests = failed,
			successRate = Math.Round(successRate, 2),
			activeFiles = _fileSemaphores.Count,
			recommendation = failed > (total * 0.05) 
				? "⚠️ High failure rate - consider increasing MAX_CONCURRENT_TILE_REQUESTS"
				: "✅ Tile server performing well"
		});
	}

		[HttpPost("/preview_tile_count")]
		public IActionResult PreviewTileCount([FromBody] PreviewRequest body)
		{
			if (body.Bounds is null || body.Zoom_Levels is null || body.Zoom_Levels.Count == 0)
			{
				return BadRequest(new { error = "Missing bounds or zoom levels" });
			}

			const double MIN_AREA_KM2 = 75.0; // Enforce minimum selectable area
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
			if (areaKm2 < MIN_AREA_KM2)
			{
				return BadRequest(new { error = $"Selected area is too small: {Math.Round(areaKm2,2)} km². Minimum allowed is {MIN_AREA_KM2} km²" });
			}

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

			// Server-side enforcement of minimum area as a safety net
			const double MIN_AREA_KM2 = 75.0;
			var areaKm2 = TileMath.CalculateAreaKm2(body.Bounds.North, body.Bounds.South, body.Bounds.East, body.Bounds.West);
			if (areaKm2 < MIN_AREA_KM2)
			{
				return BadRequest(new { error = $"Selected area is too small: {Math.Round(areaKm2,2)} km². Minimum allowed is {MIN_AREA_KM2} km²" });
			}
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
				string? mbtilesFolder = null;
				
				if (job.Format == "mbtiles")
				{
					// Pass original bounds to ensure accurate metadata
					var originalBounds = (body.Bounds.North, body.Bounds.South, body.Bounds.East, body.Bounds.West);
					var mbtilesResult = await _tiles.CreateMbtilesAsync(tiles, jobId, job.Style, p => job.Progress = p, originalBounds, body.Name);
					if (mbtilesResult != null)
					{
						resultBytes = mbtilesResult.Bytes;
						job.FileName = mbtilesResult.FileName;
						mbtilesFolder = Path.GetDirectoryName(job.FileName);
					}
				}
				else
				{
					resultBytes = await _tiles.CreateZipAsync(tiles, jobId, job.Style, p => job.Progress = p);
				}

				// Download terrain data for offline tactical analysis (only for mbtiles)
				if (job.Format == "mbtiles" && !string.IsNullOrEmpty(mbtilesFolder))
				{
					try
					{
						Console.WriteLine($"[JobsController] Starting terrain download for job {jobId}, folder: {mbtilesFolder}");
						var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
						// Convert web path (forward slashes) to OS path for file system operations
						var folderPath = mbtilesFolder.Replace('/', Path.DirectorySeparatorChar);
						var terrainDbPath = Path.Combine(wwwRoot, folderPath, "terrain.db");
						Console.WriteLine($"[JobsController] Terrain DB path: {terrainDbPath}");
						
						var terrainDataset = await _terrain.DownloadTerrainDataAsync(
							jobId,
							body.Bounds.North,
							body.Bounds.South,
							body.Bounds.East,
							body.Bounds.West,
							body.Zoom_Levels,
							job.Style,
							terrainDbPath,
							progress => { Console.WriteLine($"[JobsController] Terrain progress: {progress}"); }
						);
						
						// Store as web path (forward slashes) for frontend
						job.TerrainDataFileName = $"{mbtilesFolder}/terrain.db";
						Console.WriteLine($"[JobsController] Terrain download completed successfully");
					}
					catch (Exception terrainEx)
					{
						// Terrain data download is optional - don't fail the entire job
						// Log the error but continue
						Console.WriteLine($"[JobsController] Terrain download failed: {terrainEx.Message}");
						Console.WriteLine($"[JobsController] Stack trace: {terrainEx.StackTrace}");
						job.TerrainDataError = $"Terrain data download failed: {terrainEx.Message}";
					}
				}
				else
				{
					Console.WriteLine($"[JobsController] Skipping terrain download - Format: {job.Format}, Folder: {mbtilesFolder ?? "null"}");
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

		// ============= Consolidated MBTiles endpoints (from MbtilesController) =============

		[HttpGet("/mbtiles/list")]
		[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { })]
		public IActionResult ListMbTiles()
		{
			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			if (!Directory.Exists(wwwRoot))
			{
				return new JsonResult(new List<object>());
			}

			var maps = new List<object>();
			
			// Look for map folders
			var mapFolders = Directory.GetDirectories(wwwRoot)
				.Where(d => Directory.GetFiles(d, "map.mbtiles").Any())
				.Select(d => new DirectoryInfo(d))
				.OrderByDescending(di => di.CreationTimeUtc)
				.ToList();

			foreach (var folder in mapFolders)
			{
				var mbtilesFile = Path.Combine(folder.FullName, "map.mbtiles");
				var terrainFile = Path.Combine(folder.FullName, "terrain.db");
				
				if (System.IO.File.Exists(mbtilesFile))
				{
					var fileInfo = new FileInfo(mbtilesFile);
					var terrainExists = System.IO.File.Exists(terrainFile);
					var terrainSize = terrainExists ? new FileInfo(terrainFile).Length : 0;
					
					maps.Add(new
					{
						name = folder.Name,
						path = $"{folder.Name}/map.mbtiles", // Use forward slashes for web paths
						size = fileInfo.Length,
						terrain_size = terrainSize,
						has_terrain = terrainExists,
						created = folder.CreationTimeUtc,
						modified = folder.LastWriteTimeUtc
					});
				}
			}

			return new JsonResult(maps);
		}

	[HttpGet("/mbtiles/tile/{z}/{x}/{y}.png")]
	[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "file" })]
	public async Task<IActionResult> GetMbTile(int z, int x, int y, [FromQuery] string file)
	{
		if (string.IsNullOrEmpty(file))
		{
			return BadRequest("Missing 'file' query parameter");
		}
		
		var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
		// Convert web path (forward slashes) to OS path
		var normalizedFilename = file.Replace('/', Path.DirectorySeparatorChar);
		var filePath = Path.Combine(wwwRoot, normalizedFilename);

	if (!System.IO.File.Exists(filePath))
	{
		return NotFound("MBTiles file not found");
	}

	// Track request for monitoring
	Interlocked.Increment(ref _totalTileRequests);
	
	// Get semaphore for this file - uses configurable limit
	var semaphore = _fileSemaphores.GetOrAdd(filePath, _ => new SemaphoreSlim(MAX_CONCURRENT_TILE_REQUESTS, MAX_CONCURRENT_TILE_REQUESTS));
	
	// Use TryWait with timeout to avoid blocking - fail fast if overloaded
	if (!await semaphore.WaitAsync(TimeSpan.FromSeconds(2)))
	{
		Interlocked.Increment(ref _failedTileRequests);
		return StatusCode(503, "Tile server busy - too many concurrent requests");
	}
	
	try
	{
		// Use cached connection string for better performance
		var cs = _connectionStrings.GetOrAdd(filePath, path =>
		{
			return new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
			{
				DataSource = path,
				Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly,
				Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Shared,
				Pooling = true
			}.ToString();
		});

		// Reduced retry attempts and delays for faster response
		for (int attempt = 0; attempt < 2; attempt++)
		{
			try
			{
				await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(cs);
				await conn.OpenAsync();

				var tmsY = (1 << z) - 1 - y;

				await using var cmd = conn.CreateCommand();
				cmd.CommandText = "SELECT tile_data FROM tiles WHERE zoom_level = @z AND tile_column = @x AND tile_row = @y";
				cmd.Parameters.AddWithValue("@z", z);
				cmd.Parameters.AddWithValue("@x", x);
				cmd.Parameters.AddWithValue("@y", tmsY);

				var result = await cmd.ExecuteScalarAsync();
				if (result is byte[] tileData)
				{
					// Add aggressive caching headers
					Response.Headers["Cache-Control"] = "public, max-age=86400, immutable";
					Response.Headers["Expires"] = DateTime.UtcNow.AddDays(1).ToString("R");
					return File(tileData, "image/png");
				}
				return NotFound();
			}
			catch (System.IO.IOException ex) when (ex.Message.Contains("being used by another process") && attempt < 1)
			{
				// Reduced retry delay from 50ms to 10ms
				await Task.Delay(10);
				continue;
			}
			catch (Exception ex)
			{
				if (attempt == 1) // Last attempt
				{
					Interlocked.Increment(ref _failedTileRequests);
					return StatusCode(500, $"Error reading tile: {ex.Message}");
				}
				await Task.Delay(10);
			}
		}
		
		Interlocked.Increment(ref _failedTileRequests);
		return StatusCode(500, "Failed to read tile");
	}
	finally
	{
		semaphore.Release();
	}
	}

		[HttpGet("/mbtiles/metadata")]
		[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "file" })]
		public async Task<IActionResult> GetMbTilesMetadata([FromQuery] string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				return BadRequest("Missing 'file' query parameter");
			}
			
			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			// Convert web path (forward slashes) to OS path
			var normalizedFilename = file.Replace('/', Path.DirectorySeparatorChar);
			var filePath = Path.Combine(wwwRoot, normalizedFilename);

		if (!System.IO.File.Exists(filePath))
		{
			return NotFound("MBTiles file not found");
		}

		// Get semaphore for this file to limit concurrent access
		var semaphore = _fileSemaphores.GetOrAdd(filePath, _ => new SemaphoreSlim(5, 5)); // Max 5 concurrent metadata requests per file
		
		await semaphore.WaitAsync();
		try
		{
			// Use cached connection string for better performance
			var cs = _connectionStrings.GetOrAdd(filePath, path =>
			{
				return new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
				{
					DataSource = path,
					Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly,
					Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Shared,
					Pooling = true
				}.ToString();
			});

			// Simple retry mechanism for file lock issues
			for (int attempt = 0; attempt < 3; attempt++)
			{
				try
				{
					await using var conn = new Microsoft.Data.Sqlite.SqliteConnection(cs);
					await conn.OpenAsync();

					var metadata = new Dictionary<string, string>();
					await using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = "SELECT name, value FROM metadata";
						await using var reader = await cmd.ExecuteReaderAsync();
						while (await reader.ReadAsync())
						{
							metadata[reader.GetString(0)] = reader.GetString(1);
						}
					}

					long tileCount = 0;
					await using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = "SELECT COUNT(*) FROM tiles";
						tileCount = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
					}

					return new JsonResult(new { metadata, tile_count = tileCount });
				}
				catch (System.IO.IOException ex) when (ex.Message.Contains("being used by another process") && attempt < 2)
				{
					// Wait before retry (exponential backoff)
					await Task.Delay(50 * (attempt + 1));
					continue;
				}
				catch (Exception ex)
				{
					if (attempt == 2) // Last attempt
					{
						return StatusCode(500, new { error = $"Error reading metadata after 3 attempts: {ex.Message}" });
					}
					await Task.Delay(50 * (attempt + 1));
				}
			}
			
			return StatusCode(500, new { error = "Failed to read metadata after multiple attempts" });
		}
		finally
		{
			semaphore.Release();
		}
		}

		// ============= Consolidated Terrain offline endpoints (from TerrainController) =============

		[HttpGet("/terrain/list")]
		public IActionResult ListTerrainDatabases()
		{
			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			if (!Directory.Exists(wwwRoot))
			{
				return new JsonResult(new List<object>());
			}

			var terrainFiles = new List<object>();
			
			// Look for terrain.db files in map folders
			var mapFolders = Directory.GetDirectories(wwwRoot)
				.Where(d => Directory.GetFiles(d, "terrain.db").Any())
				.Select(d => new DirectoryInfo(d))
				.OrderByDescending(di => di.CreationTimeUtc)
				.ToList();

			foreach (var folder in mapFolders)
			{
				var terrainFile = Path.Combine(folder.FullName, "terrain.db");
				if (System.IO.File.Exists(terrainFile))
				{
					var fileInfo = new FileInfo(terrainFile);
					terrainFiles.Add(new
					{
						name = "terrain.db",
						path = $"{folder.Name}/terrain.db", // Use forward slashes for web paths
						size = fileInfo.Length,
						created = fileInfo.CreationTimeUtc,
						modified = fileInfo.LastWriteTimeUtc,
						jobId = folder.Name.Split('-')[0], // Extract job ID from folder name
						mapFolder = folder.Name
					});
				}
			}

			return new JsonResult(terrainFiles);
		}

		[HttpPost("/api/v1/elevation/lookup")]
		public async Task<IActionResult> LookupElevation([FromBody] ElevationLookupRequest request)
		{
			if (request?.Locations == null || request.Locations.Count == 0)
			{
				return BadRequest(new { error = "Missing locations" });
			}

			var terrainDb = Request.Headers["X-Terrain-Database"].FirstOrDefault() ?? Request.Query["terrain_db"].FirstOrDefault();
			if (string.IsNullOrEmpty(terrainDb))
			{
				return BadRequest(new { error = "Missing terrain database parameter (X-Terrain-Database header or terrain_db query param)" });
			}

			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			// Convert web path (forward slashes) to OS path
			var normalizedTerrainDb = terrainDb.Replace('/', Path.DirectorySeparatorChar);
			var dbPath = Path.Combine(wwwRoot, normalizedTerrainDb);

			if (!System.IO.File.Exists(dbPath))
			{
				return NotFound(new { error = "Terrain database not found" });
			}

			try
			{
				using var context = new TerrainDataContext(dbPath);
				await context.InitializeDatabaseAsync();

				var datasetId = await GetTerrainDatasetIdAsync(context);
				if (datasetId == Guid.Empty)
				{
					return NotFound(new { error = "No terrain dataset found in database" });
				}

				var results = new List<object>();
				foreach (var loc in request.Locations)
				{
					var elevationPoints = await context.QueryElevationPointsAsync(
						loc.Latitude - 0.01,
						loc.Latitude + 0.01,
						loc.Longitude - 0.01,
						loc.Longitude + 0.01,
						datasetId
					);
					double elevation = 0;
					if (elevationPoints.Any())
					{
						var closest = elevationPoints
							.OrderBy(p => Math.Pow(p.Latitude - loc.Latitude, 2) + Math.Pow(p.Longitude - loc.Longitude, 2))
							.First();
						elevation = closest.ElevationMeters;
					}
					results.Add(new { latitude = loc.Latitude, longitude = loc.Longitude, elevation });
				}

				return new JsonResult(new { results });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}

		[HttpPost("/api/terrain/features")]
		public async Task<IActionResult> QueryTerrainFeatures([FromBody] TerrainFeaturesRequest request)
		{
			if (request?.Bbox == null)
			{
				return BadRequest(new { error = "Missing bbox parameter" });
			}

			var terrainDb = Request.Headers["X-Terrain-Database"].FirstOrDefault() ?? Request.Query["terrain_db"].FirstOrDefault();
			if (string.IsNullOrEmpty(terrainDb))
			{
				return BadRequest(new { error = "Missing terrain database parameter" });
			}

			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			// Convert web path (forward slashes) to OS path
			var normalizedTerrainDb = terrainDb.Replace('/', Path.DirectorySeparatorChar);
			var dbPath = Path.Combine(wwwRoot, normalizedTerrainDb);
			if (!System.IO.File.Exists(dbPath)) return NotFound(new { error = "Terrain database not found" });

			try
			{
				using var context = new TerrainDataContext(dbPath);
				await context.InitializeDatabaseAsync();
				var datasetId = await GetTerrainDatasetIdAsync(context);
				if (datasetId == Guid.Empty) return NotFound(new { error = "No terrain dataset found in database" });

				var features = await context.QueryTerrainFeaturesAsync(
					request.Bbox[0], request.Bbox[2], request.Bbox[1], request.Bbox[3], datasetId, request.FeatureType
				);

				var elements = features.Select(f => new
				{
					id = f.OsmId,
					type = "way",
					tags = string.IsNullOrEmpty(f.TagsJson)
						? new Dictionary<string, string>()
						: System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(f.TagsJson),
					geometry = ParseGeometryToOverpassFormat(f.GeometryJson)
				}).ToList();

				return new JsonResult(new { elements });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}

		[HttpGet("/terrain/metadata")]
		public async Task<IActionResult> GetTerrainMetadata([FromQuery] string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				return BadRequest(new { error = "Missing 'file' query parameter" });
			}
			
			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			// Convert web path (forward slashes) to OS path
			var normalizedFilename = file.Replace('/', Path.DirectorySeparatorChar);
			var dbPath = Path.Combine(wwwRoot, normalizedFilename);
			if (!System.IO.File.Exists(dbPath)) return NotFound(new { error = "Terrain database not found" });

			try
			{
				using var context = new TerrainDataContext(dbPath);
				await context.InitializeDatabaseAsync();
				var datasetId = await GetTerrainDatasetIdAsync(context);
				if (datasetId == Guid.Empty) return NotFound(new { error = "No terrain dataset found in database" });
				var dataset = await GetTerrainDatasetAsync(context, datasetId);
				if (dataset == null) return NotFound(new { error = "Dataset metadata not found" });

				var bounds = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(dataset.BoundsJson);
				var zoomLevels = System.Text.Json.JsonSerializer.Deserialize<List<int>>(dataset.ZoomLevelsJson);
				return new JsonResult(new
				{
					job_id = dataset.JobId,
					file_name = dataset.FileName,
					style = dataset.Style,
					bounds = bounds,
					zoom_levels = zoomLevels,
					elevation_point_count = dataset.ElevationPointCount,
					terrain_feature_count = dataset.TerrainFeatureCount,
					grid_resolution_meters = dataset.GridResolutionMeters,
					is_complete = dataset.IsComplete,
					created_utc = dataset.CreatedUtc,
					completed_utc = dataset.CompletedUtc
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}

		private async Task<Guid> GetTerrainDatasetIdAsync(TerrainDataContext context)
		{
			var conn = context.GetConnection();
			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT id FROM terrain_datasets LIMIT 1";
			var result = await cmd.ExecuteScalarAsync();
			if (result != null && Guid.TryParse(result.ToString(), out var id)) return id;
			return Guid.Empty;
		}

		private async Task<TerrainDataset?> GetTerrainDatasetAsync(TerrainDataContext context, Guid datasetId)
		{
			var conn = context.GetConnection();
			using var cmd = conn.CreateCommand();
			cmd.CommandText = @"SELECT id, job_id, file_name, style, bounds_json, zoom_levels_json,
			       elevation_point_count, terrain_feature_count, grid_resolution_meters,
			       is_complete, created_utc, completed_utc FROM terrain_datasets WHERE id = @id";
			cmd.Parameters.AddWithValue("@id", datasetId.ToString());
			using var reader = await cmd.ExecuteReaderAsync();
			if (await reader.ReadAsync())
			{
				return new TerrainDataset
				{
					Id = Guid.Parse(reader.GetString(0)),
					JobId = reader.GetString(1),
					FileName = reader.GetString(2),
					Style = reader.GetString(3),
					BoundsJson = reader.GetString(4),
					ZoomLevelsJson = reader.GetString(5),
					ElevationPointCount = reader.GetInt32(6),
					TerrainFeatureCount = reader.GetInt32(7),
					GridResolutionMeters = reader.GetInt32(8),
					IsComplete = reader.GetInt32(9) == 1,
					CreatedUtc = DateTime.Parse(reader.GetString(10)),
					CompletedUtc = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11))
				};
			}
			return null;
		}

		private List<object> ParseGeometryToOverpassFormat(string geometryJson)
		{
			try
			{
				var geometry = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(geometryJson);
				if (geometry != null && geometry.TryGetValue("coordinates", out var coords))
				{
					var coordsArray = System.Text.Json.JsonSerializer.Deserialize<List<List<double>>>(coords.ToString() ?? "[]");
					return coordsArray?.Select(c => new { lat = c[1], lon = c[0] }).Cast<object>().ToList() ?? new List<object>();
				}
			}
			catch { }
			return new List<object>();
		}

		// ============= DTOs for terrain endpoints =============
		
		public class ElevationLookupRequest
		{
			public List<LocationDto> Locations { get; set; } = new();
		}

		public class LocationDto
		{
			public double Latitude { get; set; }
			public double Longitude { get; set; }
		}

		public class TerrainFeaturesRequest
		{
			public double[] Bbox { get; set; } = Array.Empty<double>();
			public string? FeatureType { get; set; }
		}
	}
}


