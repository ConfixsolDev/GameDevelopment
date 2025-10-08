using Microsoft.Data.Sqlite;
using System.Text.Json;
using TechWebSol.Models.MapManagement;

namespace TechWebSol.Services.MapManagement
{
	/// <summary>
	/// SQLite data context for offline terrain data storage
	/// </summary>
	public class TerrainDataContext : IDisposable
	{
		private readonly SqliteConnection _connection;
		private readonly string _dbPath;

		public TerrainDataContext(string dbPath)
		{
			_dbPath = dbPath;
			var cs = new SqliteConnectionStringBuilder
			{
				DataSource = dbPath,
				Mode = SqliteOpenMode.ReadWriteCreate,
				Cache = SqliteCacheMode.Private,
				Pooling = false
			}.ToString();
			
			_connection = new SqliteConnection(cs);
		}

		public SqliteConnection GetConnection() => _connection;

		public async Task InitializeDatabaseAsync()
		{
			await _connection.OpenAsync();

			// Enable WAL mode for better concurrency
			await using (var pragma = _connection.CreateCommand())
			{
				pragma.CommandText = @"
					PRAGMA journal_mode=WAL;
					PRAGMA synchronous=NORMAL;
					PRAGMA temp_store=MEMORY;
					PRAGMA busy_timeout=5000;
					PRAGMA cache_size=-10000;";
				await pragma.ExecuteNonQueryAsync();
			}

			// Create tables
			await using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = @"
					CREATE TABLE IF NOT EXISTS terrain_datasets (
						id TEXT PRIMARY KEY,
						job_id TEXT NOT NULL,
						file_name TEXT NOT NULL,
						style TEXT NOT NULL,
						bounds_json TEXT NOT NULL,
						zoom_levels_json TEXT NOT NULL,
						elevation_point_count INTEGER DEFAULT 0,
						terrain_feature_count INTEGER DEFAULT 0,
						grid_resolution_meters INTEGER DEFAULT 90,
						is_complete INTEGER DEFAULT 0,
						error_message TEXT,
						created_utc TEXT NOT NULL,
						modified_utc TEXT,
						completed_utc TEXT
					);

					CREATE TABLE IF NOT EXISTS elevation_points (
						id TEXT PRIMARY KEY,
						terrain_dataset_id TEXT NOT NULL,
						latitude REAL NOT NULL,
						longitude REAL NOT NULL,
						elevation_meters REAL NOT NULL,
						source TEXT NOT NULL,
						created_utc TEXT NOT NULL,
						modified_utc TEXT,
						FOREIGN KEY (terrain_dataset_id) REFERENCES terrain_datasets(id) ON DELETE CASCADE
					);

					CREATE TABLE IF NOT EXISTS terrain_features (
						id TEXT PRIMARY KEY,
						terrain_dataset_id TEXT NOT NULL,
						osm_id INTEGER NOT NULL,
						feature_type TEXT NOT NULL,
						sub_type TEXT,
						name TEXT,
						geometry_json TEXT NOT NULL,
						tags_json TEXT,
						bbox_min_lat REAL NOT NULL,
						bbox_max_lat REAL NOT NULL,
						bbox_min_lon REAL NOT NULL,
						bbox_max_lon REAL NOT NULL,
						difficulty_rating INTEGER DEFAULT 0,
						created_utc TEXT NOT NULL,
						modified_utc TEXT,
						FOREIGN KEY (terrain_dataset_id) REFERENCES terrain_datasets(id) ON DELETE CASCADE
					);

					CREATE TABLE IF NOT EXISTS coastline_segments (
						id TEXT PRIMARY KEY,
						terrain_dataset_id TEXT NOT NULL,
						osm_id INTEGER NOT NULL,
						geometry_json TEXT NOT NULL,
						bbox_min_lat REAL NOT NULL,
						bbox_max_lat REAL NOT NULL,
						bbox_min_lon REAL NOT NULL,
						bbox_max_lon REAL NOT NULL,
						created_utc TEXT NOT NULL,
						modified_utc TEXT,
						FOREIGN KEY (terrain_dataset_id) REFERENCES terrain_datasets(id) ON DELETE CASCADE
					);

					-- Spatial indices for fast queries
					CREATE INDEX IF NOT EXISTS idx_elevation_points_location 
						ON elevation_points(latitude, longitude);
					
					CREATE INDEX IF NOT EXISTS idx_elevation_points_dataset 
						ON elevation_points(terrain_dataset_id);
					
					CREATE INDEX IF NOT EXISTS idx_terrain_features_bbox 
						ON terrain_features(bbox_min_lat, bbox_max_lat, bbox_min_lon, bbox_max_lon);
					
					CREATE INDEX IF NOT EXISTS idx_terrain_features_dataset 
						ON terrain_features(terrain_dataset_id);
					
					CREATE INDEX IF NOT EXISTS idx_terrain_features_type 
						ON terrain_features(feature_type);
					
					CREATE INDEX IF NOT EXISTS idx_coastline_segments_bbox 
						ON coastline_segments(bbox_min_lat, bbox_max_lat, bbox_min_lon, bbox_max_lon);
					
					CREATE INDEX IF NOT EXISTS idx_coastline_segments_dataset 
						ON coastline_segments(terrain_dataset_id);
				";
				await cmd.ExecuteNonQueryAsync();
			}
		}

		public async Task<Guid> InsertTerrainDatasetAsync(TerrainDataset dataset)
		{
			await using var cmd = _connection.CreateCommand();
			cmd.CommandText = @"
				INSERT INTO terrain_datasets 
				(id, job_id, file_name, style, bounds_json, zoom_levels_json, 
				 grid_resolution_meters, is_complete, created_utc)
				VALUES (@id, @job_id, @file_name, @style, @bounds_json, @zoom_levels_json,
				        @grid_resolution_meters, @is_complete, @created_utc)";
			
			var id = dataset.Id == Guid.Empty ? Guid.NewGuid() : dataset.Id;
			cmd.Parameters.AddWithValue("@id", id.ToString());
			cmd.Parameters.AddWithValue("@job_id", dataset.JobId);
			cmd.Parameters.AddWithValue("@file_name", dataset.FileName);
			cmd.Parameters.AddWithValue("@style", dataset.Style);
			cmd.Parameters.AddWithValue("@bounds_json", dataset.BoundsJson);
			cmd.Parameters.AddWithValue("@zoom_levels_json", dataset.ZoomLevelsJson);
			cmd.Parameters.AddWithValue("@grid_resolution_meters", dataset.GridResolutionMeters);
			cmd.Parameters.AddWithValue("@is_complete", dataset.IsComplete ? 1 : 0);
			cmd.Parameters.AddWithValue("@created_utc", dataset.CreatedUtc.ToString("O"));
			
			await cmd.ExecuteNonQueryAsync();
			return id;
		}

		public async Task BulkInsertElevationPointsAsync(List<ElevationPoint> points)
		{
			await using var tx = await _connection.BeginTransactionAsync();
			
			await using var cmd = _connection.CreateCommand();
			cmd.Transaction = (SqliteTransaction)tx;
			cmd.CommandText = @"
				INSERT INTO elevation_points 
				(id, terrain_dataset_id, latitude, longitude, elevation_meters, source, created_utc)
				VALUES (@id, @terrain_dataset_id, @latitude, @longitude, @elevation_meters, @source, @created_utc)";
			
			var pId = cmd.Parameters.Add("@id", SqliteType.Text);
			var pDatasetId = cmd.Parameters.Add("@terrain_dataset_id", SqliteType.Text);
			var pLat = cmd.Parameters.Add("@latitude", SqliteType.Real);
			var pLon = cmd.Parameters.Add("@longitude", SqliteType.Real);
			var pElev = cmd.Parameters.Add("@elevation_meters", SqliteType.Real);
			var pSource = cmd.Parameters.Add("@source", SqliteType.Text);
			var pCreated = cmd.Parameters.Add("@created_utc", SqliteType.Text);
			
			foreach (var point in points)
			{
				pId.Value = point.Id == Guid.Empty ? Guid.NewGuid().ToString() : point.Id.ToString();
				pDatasetId.Value = point.TerrainDatasetId.ToString();
				pLat.Value = point.Latitude;
				pLon.Value = point.Longitude;
				pElev.Value = point.ElevationMeters;
				pSource.Value = point.Source;
				pCreated.Value = point.CreatedUtc.ToString("O");
				
				await cmd.ExecuteNonQueryAsync();
			}
			
			await tx.CommitAsync();
		}

		public async Task BulkInsertTerrainFeaturesAsync(List<TerrainFeature> features)
		{
			await using var tx = await _connection.BeginTransactionAsync();
			
			await using var cmd = _connection.CreateCommand();
			cmd.Transaction = (SqliteTransaction)tx;
			cmd.CommandText = @"
				INSERT INTO terrain_features 
				(id, terrain_dataset_id, osm_id, feature_type, sub_type, name, geometry_json, 
				 tags_json, bbox_min_lat, bbox_max_lat, bbox_min_lon, bbox_max_lon, 
				 difficulty_rating, created_utc)
				VALUES (@id, @terrain_dataset_id, @osm_id, @feature_type, @sub_type, @name, 
				        @geometry_json, @tags_json, @bbox_min_lat, @bbox_max_lat, 
				        @bbox_min_lon, @bbox_max_lon, @difficulty_rating, @created_utc)";
			
			var pId = cmd.Parameters.Add("@id", SqliteType.Text);
			var pDatasetId = cmd.Parameters.Add("@terrain_dataset_id", SqliteType.Text);
			var pOsmId = cmd.Parameters.Add("@osm_id", SqliteType.Integer);
			var pType = cmd.Parameters.Add("@feature_type", SqliteType.Text);
			var pSubType = cmd.Parameters.Add("@sub_type", SqliteType.Text);
			var pName = cmd.Parameters.Add("@name", SqliteType.Text);
			var pGeometry = cmd.Parameters.Add("@geometry_json", SqliteType.Text);
			var pTags = cmd.Parameters.Add("@tags_json", SqliteType.Text);
			var pMinLat = cmd.Parameters.Add("@bbox_min_lat", SqliteType.Real);
			var pMaxLat = cmd.Parameters.Add("@bbox_max_lat", SqliteType.Real);
			var pMinLon = cmd.Parameters.Add("@bbox_min_lon", SqliteType.Real);
			var pMaxLon = cmd.Parameters.Add("@bbox_max_lon", SqliteType.Real);
			var pDifficulty = cmd.Parameters.Add("@difficulty_rating", SqliteType.Integer);
			var pCreated = cmd.Parameters.Add("@created_utc", SqliteType.Text);
			
			foreach (var feature in features)
			{
				pId.Value = feature.Id == Guid.Empty ? Guid.NewGuid().ToString() : feature.Id.ToString();
				pDatasetId.Value = feature.TerrainDatasetId.ToString();
				pOsmId.Value = feature.OsmId;
				pType.Value = feature.FeatureType;
				pSubType.Value = (object?)feature.SubType ?? DBNull.Value;
				pName.Value = (object?)feature.Name ?? DBNull.Value;
				pGeometry.Value = feature.GeometryJson;
				pTags.Value = (object?)feature.TagsJson ?? DBNull.Value;
				pMinLat.Value = feature.BBoxMinLat;
				pMaxLat.Value = feature.BBoxMaxLat;
				pMinLon.Value = feature.BBoxMinLon;
				pMaxLon.Value = feature.BBoxMaxLon;
				pDifficulty.Value = feature.DifficultyRating;
				pCreated.Value = feature.CreatedUtc.ToString("O");
				
				await cmd.ExecuteNonQueryAsync();
			}
			
			await tx.CommitAsync();
		}

		public async Task<List<ElevationPoint>> QueryElevationPointsAsync(double minLat, double maxLat, double minLon, double maxLon, Guid datasetId)
		{
			var points = new List<ElevationPoint>();
			
			await using var cmd = _connection.CreateCommand();
			cmd.CommandText = @"
				SELECT id, terrain_dataset_id, latitude, longitude, elevation_meters, source, created_utc
				FROM elevation_points
				WHERE terrain_dataset_id = @dataset_id
				  AND latitude BETWEEN @min_lat AND @max_lat
				  AND longitude BETWEEN @min_lon AND @max_lon
				ORDER BY latitude, longitude";
			
			cmd.Parameters.AddWithValue("@dataset_id", datasetId.ToString());
			cmd.Parameters.AddWithValue("@min_lat", minLat);
			cmd.Parameters.AddWithValue("@max_lat", maxLat);
			cmd.Parameters.AddWithValue("@min_lon", minLon);
			cmd.Parameters.AddWithValue("@max_lon", maxLon);
			
			await using var reader = await cmd.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				points.Add(new ElevationPoint
				{
					Id = Guid.Parse(reader.GetString(0)),
					TerrainDatasetId = Guid.Parse(reader.GetString(1)),
					Latitude = reader.GetDouble(2),
					Longitude = reader.GetDouble(3),
					ElevationMeters = reader.GetDouble(4),
					Source = reader.GetString(5),
					CreatedUtc = DateTime.Parse(reader.GetString(6))
				});
			}
			
			return points;
		}

		public async Task<List<TerrainFeature>> QueryTerrainFeaturesAsync(double minLat, double maxLat, double minLon, double maxLon, Guid datasetId, string? featureType = null)
		{
			var features = new List<TerrainFeature>();
			
			var sql = @"
				SELECT id, terrain_dataset_id, osm_id, feature_type, sub_type, name, 
				       geometry_json, tags_json, bbox_min_lat, bbox_max_lat, bbox_min_lon, 
				       bbox_max_lon, difficulty_rating, created_utc
				FROM terrain_features
				WHERE terrain_dataset_id = @dataset_id
				  AND NOT (bbox_max_lat < @min_lat OR bbox_min_lat > @max_lat 
				       OR bbox_max_lon < @min_lon OR bbox_min_lon > @max_lon)";
			
			if (!string.IsNullOrEmpty(featureType))
			{
				sql += " AND feature_type = @feature_type";
			}
			
			await using var cmd = _connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("@dataset_id", datasetId.ToString());
			cmd.Parameters.AddWithValue("@min_lat", minLat);
			cmd.Parameters.AddWithValue("@max_lat", maxLat);
			cmd.Parameters.AddWithValue("@min_lon", minLon);
			cmd.Parameters.AddWithValue("@max_lon", maxLon);
			
			if (!string.IsNullOrEmpty(featureType))
			{
				cmd.Parameters.AddWithValue("@feature_type", featureType);
			}
			
			await using var reader = await cmd.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				features.Add(new TerrainFeature
				{
					Id = Guid.Parse(reader.GetString(0)),
					TerrainDatasetId = Guid.Parse(reader.GetString(1)),
					OsmId = reader.GetInt64(2),
					FeatureType = reader.GetString(3),
					SubType = reader.IsDBNull(4) ? null : reader.GetString(4),
					Name = reader.IsDBNull(5) ? null : reader.GetString(5),
					GeometryJson = reader.GetString(6),
					TagsJson = reader.IsDBNull(7) ? null : reader.GetString(7),
					BBoxMinLat = reader.GetDouble(8),
					BBoxMaxLat = reader.GetDouble(9),
					BBoxMinLon = reader.GetDouble(10),
					BBoxMaxLon = reader.GetDouble(11),
					DifficultyRating = reader.GetInt32(12),
					CreatedUtc = DateTime.Parse(reader.GetString(13))
				});
			}
			
			return features;
		}

		public async Task UpdateDatasetCompletionAsync(Guid datasetId, int elevationCount, int featureCount, bool isComplete, string? errorMessage = null)
		{
			await using var cmd = _connection.CreateCommand();
			cmd.CommandText = @"
				UPDATE terrain_datasets
				SET elevation_point_count = @elevation_count,
				    terrain_feature_count = @feature_count,
				    is_complete = @is_complete,
				    error_message = @error_message,
				    completed_utc = @completed_utc,
				    modified_utc = @modified_utc
				WHERE id = @id";
			
			cmd.Parameters.AddWithValue("@id", datasetId.ToString());
			cmd.Parameters.AddWithValue("@elevation_count", elevationCount);
			cmd.Parameters.AddWithValue("@feature_count", featureCount);
			cmd.Parameters.AddWithValue("@is_complete", isComplete ? 1 : 0);
			cmd.Parameters.AddWithValue("@error_message", (object?)errorMessage ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@completed_utc", isComplete ? DateTime.UtcNow.ToString("O") : DBNull.Value);
			cmd.Parameters.AddWithValue("@modified_utc", DateTime.UtcNow.ToString("O"));
			
			await cmd.ExecuteNonQueryAsync();
		}

		public async Task OptimizeDatabaseAsync()
		{
			await using var cmd = _connection.CreateCommand();
			cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE); VACUUM; ANALYZE;";
			await cmd.ExecuteNonQueryAsync();
		}


		public void Dispose()
		{
			try
			{
				if (_connection.State != System.Data.ConnectionState.Closed)
				{
					_connection.Close();
				}
				_connection.Dispose();
			}
			catch { }
		}
	}
}
