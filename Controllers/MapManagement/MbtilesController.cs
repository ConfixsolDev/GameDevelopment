using Microsoft.AspNetCore.Mvc;

namespace TechWebSol.Controllers.MapManagement
{
	[ApiController]
	[Route("")]
	public class MbtilesController : ControllerBase
	{
		[HttpGet("/mbtiles/list")]
		public IActionResult ListMbTiles()
		{
			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			if (!Directory.Exists(wwwRoot))
			{
				return new JsonResult(new List<object>());
			}

			var files = Directory.GetFiles(wwwRoot, "*.mbtiles")
				.Select(f => new FileInfo(f))
				.Select(fi => new
				{
					name = fi.Name,
					size = fi.Length,
					created = fi.CreationTimeUtc,
					modified = fi.LastWriteTimeUtc
				})
				.OrderByDescending(x => x.modified)
				.ToList();

			return new JsonResult(files);
		}

		[HttpGet("/mbtiles/{filename}/tile/{z}/{x}/{y}.png")]
		public async Task<IActionResult> GetMbTile(string filename, int z, int x, int y)
		{
			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			var filePath = Path.Combine(wwwRoot, filename);

			if (!System.IO.File.Exists(filePath))
			{
				return NotFound("MBTiles file not found");
			}

			var cs = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
			{
				DataSource = filePath,
				Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly,
				Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Shared
			}.ToString();

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
					return File(tileData, "image/png");
				}
				return NotFound();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error reading tile: {ex.Message}");
			}
		}

		[HttpGet("/mbtiles/{filename}/metadata")]
		public async Task<IActionResult> GetMbTilesMetadata(string filename)
		{
			var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			var filePath = Path.Combine(wwwRoot, filename);

			if (!System.IO.File.Exists(filePath))
			{
				return NotFound("MBTiles file not found");
			}

			var cs = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
			{
				DataSource = filePath,
				Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly
			}.ToString();

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
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}
	}
}


