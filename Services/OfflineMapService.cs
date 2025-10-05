using Microsoft.Data.Sqlite;
using System.Data;

namespace TechWebSol.Services
{
    public interface IOfflineMapService
    {
        Task<(byte[] data, string contentType)?> GetTileAsync(int z, int x, int y, CancellationToken ct = default);
        Task<MbtilesMetadata?> GetMetadataAsync(CancellationToken ct = default);
    }

    public class OfflineMapService : IOfflineMapService
    {
        private readonly string mbtilesPath;

        public OfflineMapService(IWebHostEnvironment env)
        {
            // Default path to App_Data/tiles/map.mbtiles
            mbtilesPath = Path.Combine(env.ContentRootPath, "App_Data", "tiles", "map.mbtiles");
        }

        public async Task<(byte[] data, string contentType)?> GetTileAsync(int z, int x, int y, CancellationToken ct = default)
        {
            try
            {
                if (!File.Exists(mbtilesPath))
                {
                    Console.WriteLine($"MBTiles file not found: {mbtilesPath}");
                    return null;
                }

                // MBTiles stores XYZ scheme by default; some sets require TMS flip for y: y = (1 << z) - 1 - y
                // We'll first try XYZ, if not found, try TMS flipped row
                var flippedY = (int)((1 << z) - 1 - y);

                Console.WriteLine($"Querying tile: z={z}, x={x}, y={y}, flippedY={flippedY}");

                await using var connection = new SqliteConnection($"Data Source={mbtilesPath};Mode=ReadOnly");
                await connection.OpenAsync(ct);

                var (image, format) = await QueryTileAsync(connection, z, x, y, ct);
                if (image == null)
                {
                    Console.WriteLine($"Tile not found with XYZ scheme, trying TMS flipped: z={z}, x={x}, y={flippedY}");
                    (image, format) = await QueryTileAsync(connection, z, x, flippedY, ct);
                }

                if (image == null)
                {
                    Console.WriteLine($"Tile not found in MBTiles database: z={z}, x={x}, y={y}");
                    return null;
                }

                var contentType = format?.ToLower() switch
                {
                    "png" => "image/png",
                    "jpg" => "image/jpeg",
                    "jpeg" => "image/jpeg",
                    "webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                Console.WriteLine($"Tile found: z={z}, x={x}, y={y}, format={format}, contentType={contentType}, size={image.Length} bytes");
                return (image!, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTileAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<MbtilesMetadata?> GetMetadataAsync(CancellationToken ct = default)
        {
            if (!File.Exists(mbtilesPath)) return null;

            await using var connection = new SqliteConnection($"Data Source={mbtilesPath};Mode=ReadOnly");
            await connection.OpenAsync(ct);

            var meta = new MbtilesMetadata();

            // Load all metadata name/value
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name, value FROM metadata";
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    var name = reader.GetString(0).ToLowerInvariant();
                    var value = reader.GetString(1);
                    switch (name)
                    {
                        case "bounds":
                            // bounds: minx, miny, maxx, maxy
                            var parts = value.Split(',');
                            if (parts.Length == 4 &&
                                double.TryParse(parts[0], out var minx) &&
                                double.TryParse(parts[1], out var miny) &&
                                double.TryParse(parts[2], out var maxx) &&
                                double.TryParse(parts[3], out var maxy))
                            {
                                meta.Bounds = new[] { minx, miny, maxx, maxy };
                            }
                            break;
                        case "center":
                            // center: lon,lat,zoom
                            var cparts = value.Split(',');
                            if (cparts.Length >= 2 &&
                                double.TryParse(cparts[0], out var lon) &&
                                double.TryParse(cparts[1], out var lat))
                            {
                                meta.Center = new[] { lon, lat };
                                if (cparts.Length >= 3 && double.TryParse(cparts[2], out var z))
                                {
                                    meta.Zoom = z;
                                }
                            }
                            break;
                        case "minzoom":
                            if (int.TryParse(value, out var minz)) meta.MinZoom = minz;
                            break;
                        case "maxzoom":
                            if (int.TryParse(value, out var maxz)) meta.MaxZoom = maxz;
                            break;
                        case "format":
                            meta.Format = value;
                            break;
                        case "name":
                            meta.Name = value;
                            break;
                        case "attribution":
                            meta.Attribution = value;
                            break;
                    }
                }
            }

            return meta;
        }

        private static async Task<(byte[]? image, string? format)> QueryTileAsync(SqliteConnection connection, int z, int x, int y, CancellationToken ct)
        {
            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT tile_data FROM tiles WHERE zoom_level = $z AND tile_column = $x AND tile_row = $y LIMIT 1";
                cmd.Parameters.AddWithValue("$z", z);
                cmd.Parameters.AddWithValue("$x", x);
                cmd.Parameters.AddWithValue("$y", y);

                await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
                if (await reader.ReadAsync(ct))
                {
                    var bytes = (byte[])reader[0];

                    // Attempt to read format from metadata (optional)
                    string? format = await GetFormatAsync(connection, ct);
                    return (bytes, format);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in QueryTileAsync: {ex.Message}");
                Console.WriteLine($"Parameters: z={z}, x={x}, y={y}");
                throw; // Re-throw to be caught by the calling method
            }
        }

        private static async Task<string?> GetFormatAsync(SqliteConnection connection, CancellationToken ct)
        {
            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT value FROM metadata WHERE name = 'format' LIMIT 1";
                var result = await cmd.ExecuteScalarAsync(ct);
                return result?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }

    public class MbtilesMetadata
    {
        public double[]? Bounds { get; set; } // [minx, miny, maxx, maxy]
        public double[]? Center { get; set; } // [lon, lat]
        public double? Zoom { get; set; }
        public int? MinZoom { get; set; }
        public int? MaxZoom { get; set; }
        public string? Format { get; set; }
        public string? Name { get; set; }
        public string? Attribution { get; set; }
    }
}


