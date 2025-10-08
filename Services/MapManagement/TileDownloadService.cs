using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Net;
using System.Threading.Channels;

namespace TechWebSol.Services.MapManagement
{
    public class MbtilesFile
    {
        public byte[]? Bytes { get; set; }
        public string? FileName { get; set; }
    }

    public class TileService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TileService> _logger;
        private const int MaxWorkers = 4;
        private const double RequestDelaySeconds = 0.1;

        private static readonly Dictionary<string, string> TileServers = new()
        {
            ["map"] = "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
            ["satellite"] = "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}"
        };

        public TileService(HttpClient httpClient, ILogger<TileService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<byte[]?> DownloadTileWithRetryAsync(string url, int maxRetries = 3)
        {
            for (var attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var jitter = Random.Shared.NextDouble() * 0.1; // 0..0.1s
                    await Task.Delay(TimeSpan.FromSeconds(RequestDelaySeconds + jitter));

                    _logger.LogDebug("Fetching tile attempt {Attempt}/{Max} {Url}", attempt + 1, maxRetries, url);
                    using var resp = await _httpClient.GetAsync(url);
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogTrace("Fetched OK {Url} length={Length}", url, resp.Content.Headers.ContentLength);
                        return await resp.Content.ReadAsByteArrayAsync();
                    }
                    else if ((int)resp.StatusCode == 429)
                    {
                        var wait = Math.Pow(2, attempt) + Random.Shared.NextDouble();
                        _logger.LogWarning("HTTP 429 for {Url}. Backing off {WaitSeconds:F2}s (attempt {Attempt}/{Max})", url, wait, attempt + 1, maxRetries);
                        await Task.Delay(TimeSpan.FromSeconds(wait));
                        continue;
                    }
                    else if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogInformation("Tile not found {Url}", url);
                        return null;
                    }
                    else
                    {
                        if (attempt < maxRetries - 1)
                        {
                            var wait = Math.Pow(2, attempt) + Random.Shared.NextDouble();
                            _logger.LogWarning("HTTP {Status} for {Url}. Retrying in {WaitSeconds:F2}s (attempt {Attempt}/{Max})", (int)resp.StatusCode, url, wait, attempt + 1, maxRetries);
                            await Task.Delay(TimeSpan.FromSeconds(wait));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error downloading {Url} on attempt {Attempt}/{Max}", url, attempt + 1, maxRetries);
                    if (attempt == maxRetries - 1) return null;
                    var wait = Math.Pow(2, attempt) + Random.Shared.NextDouble();
                    await Task.Delay(TimeSpan.FromSeconds(wait));
                }
            }
            return null;
        }

        private static string GetTilePath(string style, int z, int x, int y)
            => Path.Combine("tiles", style, z.ToString(), x.ToString(), $"{y}.png");

        private static string UrlFor(string style, int z, int x, int y)
        {
            var template = TileServers.TryGetValue(style, out var t) ? t : TileServers["map"];
            return template.Replace("{z}", z.ToString())
                           .Replace("{x}", x.ToString())
                           .Replace("{y}", y.ToString());
        }

        private async Task<(byte[]? Data, bool Cached)> DownloadSingleTileAsync(int z, int x, int y, string style)
        {
            var tilePath = GetTilePath(style, z, x, y);
            if (File.Exists(tilePath))
            {
                _logger.LogTrace("Cache hit {Path}", tilePath);
                return (await File.ReadAllBytesAsync(tilePath), true);
            }

            var url = UrlFor(style, z, x, y);
            var data = await DownloadTileWithRetryAsync(url);
            if (data is not null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(tilePath)!);
                await File.WriteAllBytesAsync(tilePath, data);
                _logger.LogDebug("Saved tile z={Z} x={X} y={Y} style={Style} -> {Path}", z, x, y, style, tilePath);
            }
            return (data, false);
        }

        public async Task<byte[]?> CreateZipAsync(List<(int z, int x, int y)> tiles, string jobId, string style, Action<int> onProgress)
        {
            var completed = 0;
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                var archiveLock = new SemaphoreSlim(1, 1); // ZipArchive is not thread-safe for concurrent writes
                var throttler = new SemaphoreSlim(MaxWorkers);
                var tasks = new List<Task>();
                foreach (var (z, x, y) in tiles)
                {
                    await throttler.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var (data, _) = await DownloadSingleTileAsync(z, x, y, style);
                            if (data is not null)
                            {
                                await archiveLock.WaitAsync();
                                try
                                {
                                    var entry = archive.CreateEntry($"{z}/{x}/{y}.png", CompressionLevel.Optimal);
                                    await using var es = entry.Open();
                                    await es.WriteAsync(data, 0, data.Length);
                                }
                                finally
                                {
                                    archiveLock.Release();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "ZIP write failed for z={Z} x={X} y={Y} style={Style}", z, x, y, style);
                        }
                        finally
                        {
                            var p = Interlocked.Increment(ref completed);
                            onProgress(p);
                            throttler.Release();
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }
            return ms.ToArray();
        }

        public async Task<MbtilesFile?> CreateMbtilesAsync(List<(int z, int x, int y)> tiles, string jobId, string style, Action<int> onProgress, (double north, double south, double east, double west)? originalBounds = null)
        {
            var contentRoot = Directory.GetCurrentDirectory();
            var wwwRoot = Path.Combine(contentRoot, "wwwroot");
            Directory.CreateDirectory(wwwRoot);
            var finalFileName = $"{jobId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}.mbtiles";
            var finalPath = Path.Combine(wwwRoot, finalFileName);
            var stagingDir = Path.Combine(contentRoot, "App_Data", "mb_staging");
            
            // Ensure staging directory exists with proper error handling
            try
            {
                if (!Directory.Exists(stagingDir))
                {
            Directory.CreateDirectory(stagingDir);
                    _logger.LogInformation("Created staging directory: {StagingDir}", stagingDir);
                }
                
                // Verify directory is writable
                var testFile = Path.Combine(stagingDir, $"test_{Guid.NewGuid():N}.tmp");
                await File.WriteAllTextAsync(testFile, "test");
                File.Delete(testFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or access staging directory: {StagingDir}", stagingDir);
                throw new IOException($"Cannot create staging directory at {stagingDir}. Please ensure the application has write permissions.", ex);
            }
            
            var tempPath = Path.Combine(stagingDir, $"{jobId}.{Guid.NewGuid():N}.mbtiles");
            _logger.LogInformation("Creating MBTiles database at: {TempPath}", tempPath);
            
            var cs = new SqliteConnectionStringBuilder { DataSource = tempPath, Mode = SqliteOpenMode.ReadWriteCreate, Cache = SqliteCacheMode.Private, Pooling = false }.ToString();
            int completed = 0;

            // Calculate bounds and zoom levels for metadata
            int minZoom = tiles.Min(t => t.z);
            int maxZoom = tiles.Max(t => t.z);
            
            double minLat, maxLat, minLon, maxLon, centerLon, centerLat;
            int centerZoom;
            
            // Use original bounds if provided, otherwise calculate from tiles
            if (originalBounds.HasValue)
            {
                minLat = originalBounds.Value.south;
                maxLat = originalBounds.Value.north;
                minLon = originalBounds.Value.west;
                maxLon = originalBounds.Value.east;
                centerLon = (minLon + maxLon) / 2.0;
                centerLat = (minLat + maxLat) / 2.0;
                centerZoom = Math.Max(minZoom, Math.Min(maxZoom, (minZoom + maxZoom) / 2));
            }
            else
            {
                // Fallback: calculate from tiles (less accurate)
                minLat = double.MaxValue;
                maxLat = double.MinValue;
                minLon = double.MaxValue;
                maxLon = double.MinValue;

                foreach (var (z, x, y) in tiles)
                {
                    var n = Math.Pow(2, z);
                    var lonDeg = x / n * 360.0 - 180.0;
                    var latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
                    var latDeg = latRad * 180.0 / Math.PI;
                    
                    minLon = Math.Min(minLon, lonDeg);
                    maxLon = Math.Max(maxLon, lonDeg);
                    minLat = Math.Min(minLat, latDeg);
                    maxLat = Math.Max(maxLat, latDeg);
                }

                centerLon = (minLon + maxLon) / 2.0;
                centerLat = (minLat + maxLat) / 2.0;
                centerZoom = (minZoom + maxZoom) / 2;
            }

            await using (var conn = new SqliteConnection(cs))
            {
                try
            {
                await conn.OpenAsync();
                    _logger.LogInformation("SQLite connection opened successfully");
                }
                catch (SqliteException ex)
                {
                    _logger.LogError(ex, "Failed to open SQLite database at {TempPath}. Error code: {ErrorCode}", tempPath, ex.SqliteErrorCode);
                    throw new IOException($"Cannot create SQLite database at {tempPath}. Error code: {ex.SqliteErrorCode}. Please ensure the directory exists and has write permissions.", ex);
                }
                
                // Set SQLite pragmas for better performance and reliability
                await using (var pragma = conn.CreateCommand())
                {
                    pragma.CommandText = @"
                        PRAGMA journal_mode=WAL;
                        PRAGMA synchronous=NORMAL;
                        PRAGMA temp_store=MEMORY;
                        PRAGMA busy_timeout=5000;
                        PRAGMA cache_size=-10000;";
                    await pragma.ExecuteNonQueryAsync();
                }

                // Create tables with proper schema
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE metadata (name TEXT PRIMARY KEY, value TEXT);
                        CREATE TABLE tiles (
                            zoom_level INTEGER NOT NULL,
                            tile_column INTEGER NOT NULL,
                            tile_row INTEGER NOT NULL,
                            tile_data BLOB NOT NULL,
                            PRIMARY KEY (zoom_level, tile_column, tile_row)
                        );";
                    await cmd.ExecuteNonQueryAsync();
                }

                // Insert metadata with all required fields
                await using (var tx = await conn.BeginTransactionAsync())
                {
                    await using var meta = conn.CreateCommand();
                    meta.Transaction = (SqliteTransaction)tx;
                    meta.CommandText = "INSERT INTO metadata (name, value) VALUES (@n, @v)";
                    var pName = meta.Parameters.Add("@n", SqliteType.Text);
                    var pValue = meta.Parameters.Add("@v", SqliteType.Text);
                    
                    async Task InsertMetadata(string name, string value)
                    {
                        pName.Value = name;
                        pValue.Value = value;
                        await meta.ExecuteNonQueryAsync();
                    }

                    var mapName = $"Offline Map - {(style == "satellite" ? "Satellite" : "Street")}";
                    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    await InsertMetadata("name", mapName);
                    await InsertMetadata("type", "baselayer");
                    await InsertMetadata("version", "1.3.0");
                    await InsertMetadata("description", $"Offline map tiles - {style} style, generated {timestamp}");
                    await InsertMetadata("format", "png");
                    await InsertMetadata("scheme", "tms"); // TMS scheme as we're using TMS Y coordinate
                    // Use standard MBTiles 1.3 format: comma-separated values for bounds and center
                    await InsertMetadata("bounds", $"{minLon:F7},{minLat:F7},{maxLon:F7},{maxLat:F7}");
                    await InsertMetadata("center", $"{centerLon:F7},{centerLat:F7},{centerZoom}");
                    await InsertMetadata("minzoom", minZoom.ToString());
                    await InsertMetadata("maxzoom", maxZoom.ToString());
                    await InsertMetadata("attribution", style == "satellite" 
                        ? "<a href='https://www.esri.com'>Esri</a>" 
                        : "<a href='https://www.openstreetmap.org/copyright'>© OpenStreetMap contributors</a>");
                    
                    // Add JSON format for better compatibility with MapTiler Cloud and modern tools
                    await InsertMetadata("json", System.Text.Json.JsonSerializer.Serialize(new
                    {
                        name = mapName,
                        description = $"Offline map tiles - {style} style, generated {timestamp}",
                        version = "1.3.0",
                        tilejson = "2.2.0",
                        scheme = "tms",
                        tiles = new string[] { },
                        type = "baselayer",
                        format = "png",
                        bounds = new[] { minLon, minLat, maxLon, maxLat },
                        center = new[] { centerLon, centerLat, (double)centerZoom },
                        minzoom = minZoom,
                        maxzoom = maxZoom,
                        attribution = style == "satellite" 
                            ? "<a href='https://www.esri.com'>Esri</a>" 
                            : "<a href='https://www.openstreetmap.org/copyright'>© OpenStreetMap contributors</a>",
                        generator = "OfflineMapDownloader.NET/1.0"
                    }));

                    await tx.CommitAsync();
                }

                var channel = Channel.CreateBounded<(int z, int x, int y, byte[] data)>(
                    new BoundedChannelOptions(Math.Max(tiles.Count / 8, 256)) 
                    { 
                        SingleReader = true, 
                        SingleWriter = false, 
                        FullMode = BoundedChannelFullMode.Wait 
                    });

                var writerTask = Task.Run(async () =>
                {
                    await using var tx = await conn.BeginTransactionAsync();
                    await using var insert = conn.CreateCommand();
                    insert.Transaction = (SqliteTransaction)tx;
                    insert.CommandText = "INSERT OR REPLACE INTO tiles (zoom_level, tile_column, tile_row, tile_data) VALUES ($z, $x, $y, $d)";
                    var pz = insert.Parameters.Add("$z", SqliteType.Integer);
                    var px = insert.Parameters.Add("$x", SqliteType.Integer);
                    var py = insert.Parameters.Add("$y", SqliteType.Integer);
                    var pd = insert.Parameters.Add("$d", SqliteType.Blob);
                    
                    await foreach (var (z, x, y, data) in channel.Reader.ReadAllAsync())
                    {
                        // Convert from XYZ to TMS coordinate system
                        var tmsY = (int)(Math.Pow(2, z) - 1) - y;
                        pz.Value = z;
                        px.Value = x;
                        py.Value = tmsY;
                        pd.Value = data;
                        await insert.ExecuteNonQueryAsync();
                        var p = Interlocked.Increment(ref completed);
                        onProgress(p);
                    }
                    
                    await tx.CommitAsync();
                });

                var throttler = new SemaphoreSlim(MaxWorkers);
                var producerTasks = new List<Task>(tiles.Count);
                
                foreach (var (z, x, y) in tiles)
                {
                    await throttler.WaitAsync();
                    producerTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var (data, _) = await DownloadSingleTileAsync(z, x, y, style);
                            if (data is not null) 
                            {
                                await channel.Writer.WriteAsync((z, x, y, data));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Tile download failed z={Z} x={X} y={Y} style={Style}", z, x, y, style);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(producerTasks);
                channel.Writer.Complete();
                await writerTask;

                // Optimize database before closing
                try
                {
                    await using (var optimize = conn.CreateCommand())
                    {
                        optimize.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                        await optimize.ExecuteNonQueryAsync();
                        
                        optimize.CommandText = "VACUUM;";
                        await optimize.ExecuteNonQueryAsync();
                        
                        optimize.CommandText = "ANALYZE;";
                        await optimize.ExecuteNonQueryAsync();
                    }
                    _logger.LogInformation("MBTiles database optimized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to optimize MBTiles database, but continuing");
                }

                // Ensure all changes are written to disk
                await conn.CloseAsync();
            }

            // CRITICAL: Ensure SQLite connection is fully released before reading file
            SqliteConnection.ClearAllPools();
            await Task.Delay(200); // Give OS time to release file handles
            GC.Collect(); // Force garbage collection to ensure connections are disposed
            GC.WaitForPendingFinalizers();

            // Verify the file exists and is valid
            if (!File.Exists(tempPath))
            {
                _logger.LogError("MBTiles file was not created at {TempPath}", tempPath);
                return null;
            }

            var fileInfo = new FileInfo(tempPath);
            _logger.LogInformation("MBTiles file created: {TempPath}, Size: {Size} bytes", tempPath, fileInfo.Length);

            // Validate the MBTiles file before reading
            try
            {
                var validateCs = new SqliteConnectionStringBuilder 
                { 
                    DataSource = tempPath, 
                    Mode = SqliteOpenMode.ReadOnly 
                }.ToString();
                
                await using var validateConn = new SqliteConnection(validateCs);
                await validateConn.OpenAsync();
                
                await using var validateCmd = validateConn.CreateCommand();
                validateCmd.CommandText = "SELECT COUNT(*) FROM tiles; SELECT COUNT(*) FROM metadata;";
                await using var reader = await validateCmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var tileCount = reader.GetInt64(0);
                    _logger.LogInformation("MBTiles validation: {TileCount} tiles in database", tileCount);
                }
                
                await validateConn.CloseAsync();
                SqliteConnection.ClearAllPools();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MBTiles validation failed - file may be corrupted");
                return null;
            }

            // Now read the file after connection is completely closed
            byte[] bytes = await File.ReadAllBytesAsync(tempPath);
            _logger.LogInformation("MBTiles file read successfully: {ByteCount} bytes", bytes.Length);

            // Move file to final location and clean up any WAL files
            try
            {
                // Clean up any WAL/SHM files that might be left behind
                var walFile = tempPath + "-wal";
                var shmFile = tempPath + "-shm";
                if (File.Exists(walFile))
                {
                    try { File.Delete(walFile); } catch { }
                }
                if (File.Exists(shmFile))
                {
                    try { File.Delete(shmFile); } catch { }
                }

#if NET8_0_OR_GREATER
                File.Move(tempPath, finalPath, overwrite: true);
#else
                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }
                File.Move(tempPath, finalPath);
#endif
                _logger.LogInformation("MBTiles file moved to: {FinalPath}", finalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move MBTiles file from {TempPath} to {FinalPath}", tempPath, finalPath);
            }

            return new MbtilesFile
            {
                Bytes = bytes,
                FileName = finalFileName
            };
        }




    }
}
