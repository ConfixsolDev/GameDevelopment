using System.Text.Json;
using System.Text.Json.Serialization;
using TechWebSol.Models.MapManagement;

namespace TechWebSol.Services.MapManagement
{
	public class TerrainDownloadService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<TerrainDownloadService> _logger;

		// Use public SRTM data service (open-elevation.com alternative using local SRTM data)
		// For production, consider hosting your own elevation server
		private const string ELEVATION_API_URL = "https://api.open-elevation.com/api/v1/lookup";
		private const string OVERPASS_API_URL = "https://overpass-api.de/api/interpreter";

		public TerrainDownloadService(HttpClient httpClient, ILogger<TerrainDownloadService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		/// <summary>
		/// Downloads terrain data for the specified bounds and saves to database
		/// </summary>
		public async Task<TerrainDataset> DownloadTerrainDataAsync(
			string jobId,
			double north,
			double south,
			double east,
			double west,
			List<int> zoomLevels,
			string style,
			string dbFilePath,
			Action<string>? onProgress = null)
		{
			onProgress?.Invoke("Creating terrain database...");

			var dataset = new TerrainDataset
			{
				JobId = jobId,
				FileName = Path.GetFileName(dbFilePath),
				Style = style,
				BoundsJson = JsonSerializer.Serialize(new { north, south, east, west }),
				ZoomLevelsJson = JsonSerializer.Serialize(zoomLevels),
				GridResolutionMeters = 90, // SRTM 90m resolution
				IsComplete = false
			};

			try
			{
				// Initialize database
				using var context = new TerrainDataContext(dbFilePath);
				await context.InitializeDatabaseAsync();

				var datasetId = await context.InsertTerrainDatasetAsync(dataset);
				dataset.Id = datasetId;

				_logger.LogInformation("Starting terrain data download for bounds: N={North}, S={South}, E={East}, W={West}",
					north, south, east, west);

				// Step 1: Download elevation data
				onProgress?.Invoke("Downloading elevation data...");
				var elevationPoints = await DownloadElevationDataAsync(north, south, east, west, datasetId);
				_logger.LogInformation("Downloaded {Count} elevation points", elevationPoints.Count);

				if (elevationPoints.Count > 0)
				{
					onProgress?.Invoke($"Saving {elevationPoints.Count} elevation points...");
					await context.BulkInsertElevationPointsAsync(elevationPoints);
				}

				// Step 2: Download terrain features from OSM
				onProgress?.Invoke("Downloading terrain features from OpenStreetMap...");
				var terrainFeatures = await DownloadTerrainFeaturesAsync(north, south, east, west, datasetId);
				_logger.LogInformation("Downloaded {Count} terrain features", terrainFeatures.Count);

				if (terrainFeatures.Count > 0)
				{
					onProgress?.Invoke($"Saving {terrainFeatures.Count} terrain features...");
					await context.BulkInsertTerrainFeaturesAsync(terrainFeatures);
				}

				// Step 3: Mark as complete
				onProgress?.Invoke("Finalizing terrain database...");
				await context.UpdateDatasetCompletionAsync(datasetId, elevationPoints.Count, terrainFeatures.Count, true);
				await context.OptimizeDatabaseAsync();

				dataset.ElevationPointCount = elevationPoints.Count;
				dataset.TerrainFeatureCount = terrainFeatures.Count;
				dataset.IsComplete = true;
				dataset.CompletedUtc = DateTime.UtcNow;

				_logger.LogInformation("Terrain data download completed successfully");
				onProgress?.Invoke("Terrain data download complete!");

				return dataset;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to download terrain data");
				dataset.IsComplete = false;
				dataset.ErrorMessage = ex.Message;
				throw;
			}
		}

		/// <summary>
		/// Downloads elevation data in a grid pattern
		/// </summary>
		private async Task<List<ElevationPoint>> DownloadElevationDataAsync(
			double north,
			double south,
			double east,
			double west,
			Guid datasetId)
		{
			var points = new List<ElevationPoint>();

			// Calculate grid resolution (approximately 90m = 0.0008 degrees at equator)
			const double gridSpacingDegrees = 0.0008;
			const int maxPointsPerRequest = 200; // API limit
			const int maxTotalPoints = 10000; // Reasonable limit for storage

			// Calculate grid dimensions
			var latSteps = (int)Math.Ceiling((north - south) / gridSpacingDegrees);
			var lonSteps = (int)Math.Ceiling((east - west) / gridSpacingDegrees);
			var totalPoints = latSteps * lonSteps;

			// Limit total points
			if (totalPoints > maxTotalPoints)
			{
				var reductionFactor = Math.Sqrt((double)maxTotalPoints / totalPoints);
				latSteps = (int)(latSteps * reductionFactor);
				lonSteps = (int)(lonSteps * reductionFactor);
			}

			_logger.LogInformation("Elevation grid: {LatSteps} x {LonSteps} = {Total} points", 
				latSteps, lonSteps, latSteps * lonSteps);

			// Generate grid points
			var locations = new List<LocationRequest>();
			for (int i = 0; i <= latSteps; i++)
			{
				for (int j = 0; j <= lonSteps; j++)
				{
					var lat = south + (north - south) * i / latSteps;
					var lon = west + (east - west) * j / lonSteps;
					locations.Add(new LocationRequest { Latitude = lat, Longitude = lon });
				}
			}

			// Download in batches
			for (int i = 0; i < locations.Count; i += maxPointsPerRequest)
			{
				var batch = locations.Skip(i).Take(maxPointsPerRequest).ToList();
				
				try
				{
					var request = new ElevationRequest { Locations = batch };
					var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
					{ 
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
					});
					
					var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
					var response = await _httpClient.PostAsync(ELEVATION_API_URL, content);
					
					if (response.IsSuccessStatusCode)
					{
						var responseJson = await response.Content.ReadAsStringAsync();
						var result = JsonSerializer.Deserialize<ElevationResponse>(responseJson, new JsonSerializerOptions 
						{ 
							PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
						});

						if (result?.Results != null)
						{
							foreach (var r in result.Results)
							{
								points.Add(new ElevationPoint
								{
									TerrainDatasetId = datasetId,
									Latitude = r.Latitude,
									Longitude = r.Longitude,
									ElevationMeters = r.Elevation,
									Source = "SRTM"
								});
							}
						}
					}
					
					// Rate limiting
					await Task.Delay(100);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to download elevation batch {Start}-{End}", i, i + batch.Count);
				}
			}

			return points;
		}

		/// <summary>
		/// Downloads terrain features from OpenStreetMap Overpass API
		/// </summary>
		private async Task<List<TerrainFeature>> DownloadTerrainFeaturesAsync(
			double north,
			double south,
			double east,
			double west,
			Guid datasetId)
		{
			var features = new List<TerrainFeature>();
			var bbox = $"{south},{west},{north},{east}";

			var overpassQuery = $@"
				[out:json][timeout:60][bbox:{bbox}];
				(
					way[""natural""=""water""];
					way[""waterway""];
					relation[""natural""=""water""];
					way[""natural""=""wetland""];
					way[""landuse""=""forest""];
					way[""natural""=""wood""];
					way[""natural""=""scrub""];
					way[""landuse""=""military""];
					way[""natural""=""sand""];
					way[""natural""=""desert""];
					way[""landuse""=""residential""];
					way[""landuse""=""industrial""];
					way[""highway""][""surface""~""unpaved|gravel|dirt|sand|grass""];
					way[""natural""=""cliff""];
					way[""natural""=""rock""];
					way[""landuse""=""quarry""];
					way[""natural""=""coastline""];
				);
				out geom;
			";

			try
			{
				var content = new StringContent(overpassQuery, System.Text.Encoding.UTF8, "text/plain");
				var response = await _httpClient.PostAsync(OVERPASS_API_URL, content);

				if (response.IsSuccessStatusCode)
				{
					var responseJson = await response.Content.ReadAsStringAsync();
					var result = JsonSerializer.Deserialize<OverpassResponse>(responseJson);

					if (result?.Elements != null)
					{
						foreach (var element in result.Elements)
						{
							if (element.Geometry == null || element.Geometry.Count < 2)
								continue;

							var featureType = DetermineFeatureType(element.Tags);
							if (string.IsNullOrEmpty(featureType))
								continue;

							// Calculate bounding box
							var lats = element.Geometry.Select(g => g.Lat).ToList();
							var lons = element.Geometry.Select(g => g.Lon).ToList();

							// Create GeoJSON geometry
							var coordinates = element.Geometry.Select(g => new[] { g.Lon, g.Lat }).ToList();
							var geometryJson = JsonSerializer.Serialize(new
							{
								type = "LineString",
								coordinates
							});

							features.Add(new TerrainFeature
							{
								TerrainDatasetId = datasetId,
								OsmId = element.Id,
								FeatureType = featureType,
								SubType = GetSubType(element.Tags, featureType),
								Name = element.Tags?.GetValueOrDefault("name"),
								GeometryJson = geometryJson,
								TagsJson = JsonSerializer.Serialize(element.Tags ?? new Dictionary<string, string>()),
								BBoxMinLat = lats.Min(),
								BBoxMaxLat = lats.Max(),
								BBoxMinLon = lons.Min(),
								BBoxMaxLon = lons.Max(),
								DifficultyRating = CalculateDifficultyRating(featureType, element.Tags)
							});
						}

						_logger.LogInformation("Processed {Count} OSM elements", result.Elements.Count);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to download terrain features from Overpass API");
				// Don't throw - terrain features are optional
			}

			return features;
		}

		private string DetermineFeatureType(Dictionary<string, string>? tags)
		{
			if (tags == null) return string.Empty;

			if (tags.ContainsKey("natural"))
			{
				var natural = tags["natural"];
				if (natural == "water") return "water";
				if (natural == "wetland") return "wetland";
				if (natural == "cliff" || natural == "rock") return "cliff";
				if (natural == "sand" || natural == "desert") return "desert";
				if (natural == "wood" || natural == "scrub") return "forest";
				if (natural == "coastline") return "coastline";
			}

			if (tags.ContainsKey("waterway")) return "water";

			if (tags.ContainsKey("landuse"))
			{
				var landuse = tags["landuse"];
				if (landuse == "forest") return "forest";
				if (landuse == "military") return "military";
				if (landuse == "residential" || landuse == "industrial") return "urban";
				if (landuse == "quarry") return "cliff";
			}

			if (tags.ContainsKey("highway") && tags.ContainsKey("surface"))
			{
				return "road";
			}

			return string.Empty;
		}

		private string? GetSubType(Dictionary<string, string>? tags, string featureType)
		{
			if (tags == null) return null;

			return featureType switch
			{
				"water" => tags.GetValueOrDefault("waterway") ?? tags.GetValueOrDefault("natural"),
				"road" => tags.GetValueOrDefault("surface"),
				"forest" => tags.GetValueOrDefault("natural") ?? tags.GetValueOrDefault("landuse"),
				_ => null
			};
		}

		private int CalculateDifficultyRating(string featureType, Dictionary<string, string>? tags)
		{
			return featureType switch
			{
				"water" => 10,      // Impassable
				"cliff" => 10,      // Impassable
				"coastline" => 10,  // Impassable (open sea)
				"wetland" => 8,     // Very difficult
				"military" => 9,    // Restricted
				"desert" => 6,      // Difficult
				"forest" => 4,      // Moderate
				"urban" => 3,       // Easy-moderate
				"road" => tags?.GetValueOrDefault("surface") switch
				{
					"unpaved" => 3,
					"gravel" => 2,
					"dirt" => 4,
					"sand" => 6,
					"grass" => 3,
					_ => 2
				},
				_ => 0
			};
		}

		// DTOs for API requests/responses
		private class LocationRequest
		{
			[JsonPropertyName("latitude")]
			public double Latitude { get; set; }
			
			[JsonPropertyName("longitude")]
			public double Longitude { get; set; }
		}

		private class ElevationRequest
		{
			[JsonPropertyName("locations")]
			public List<LocationRequest> Locations { get; set; } = new();
		}

		private class ElevationResponse
		{
			[JsonPropertyName("results")]
			public List<ElevationResult>? Results { get; set; }
		}

		private class ElevationResult
		{
			[JsonPropertyName("latitude")]
			public double Latitude { get; set; }
			
			[JsonPropertyName("longitude")]
			public double Longitude { get; set; }
			
			[JsonPropertyName("elevation")]
			public double Elevation { get; set; }
		}

		private class OverpassResponse
		{
			[JsonPropertyName("elements")]
			public List<OverpassElement>? Elements { get; set; }
		}

		private class OverpassElement
		{
			[JsonPropertyName("id")]
			public long Id { get; set; }
			
			[JsonPropertyName("tags")]
			public Dictionary<string, string>? Tags { get; set; }
			
			[JsonPropertyName("geometry")]
			public List<OverpassNode>? Geometry { get; set; }
		}

		private class OverpassNode
		{
			[JsonPropertyName("lat")]
			public double Lat { get; set; }
			
			[JsonPropertyName("lon")]
			public double Lon { get; set; }
		}
	}
}
