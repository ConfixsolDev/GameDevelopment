namespace TechWebSol.Models.MapManagement
{
	
    public abstract class BaseEntityMap
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedUtc { get; set; }
    }

    public class TerrainDataset : BaseEntityMap
	{
		/// <summary>
		/// Job ID this terrain data belongs to (links to DownloadJob)
		/// </summary>
		public string JobId { get; set; } = string.Empty;

		/// <summary>
		/// Database filename (e.g., "jobid-timestamp.terrain.db")
		/// </summary>
		public string FileName { get; set; } = string.Empty;

		/// <summary>
		/// Map style: "map" or "satellite"
		/// </summary>
		public string Style { get; set; } = "map";

		/// <summary>
		/// Geographic bounds (JSON: {north, south, east, west})
		/// </summary>
		public string BoundsJson { get; set; } = string.Empty;

		/// <summary>
		/// Zoom levels covered (JSON array: [12,13,14])
		/// </summary>
		public string ZoomLevelsJson { get; set; } = string.Empty;

		/// <summary>
		/// Total elevation points stored
		/// </summary>
		public int ElevationPointCount { get; set; }

		/// <summary>
		/// Total terrain features stored (water, forests, etc.)
		/// </summary>
		public int TerrainFeatureCount { get; set; }

		/// <summary>
		/// Grid resolution in meters (default: 90m SRTM resolution)
		/// </summary>
		public int GridResolutionMeters { get; set; } = 90;

		/// <summary>
		/// Download completed successfully
		/// </summary>
		public bool IsComplete { get; set; }

		/// <summary>
		/// Error message if download failed
		/// </summary>
		public string? ErrorMessage { get; set; }

		public DateTime? CompletedUtc { get; set; }
	}

	/// <summary>
	/// Stores elevation data in a grid pattern
	/// </summary>
	public class ElevationPoint : BaseEntityMap
	{
		/// <summary>
		/// Reference to parent terrain dataset
		/// </summary>
		public Guid TerrainDatasetId { get; set; }

		/// <summary>
		/// Latitude (WGS84)
		/// </summary>
		public double Latitude { get; set; }

		/// <summary>
		/// Longitude (WGS84)
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// Elevation in meters (can be negative for below sea level)
		/// </summary>
		public double ElevationMeters { get; set; }

		/// <summary>
		/// Data source (e.g., "SRTM", "ASTER", "Copernicus")
		/// </summary>
		public string Source { get; set; } = "SRTM";
	}

	/// <summary>
	/// Stores terrain features from OpenStreetMap (obstacles, terrain types)
	/// </summary>
	public class TerrainFeature : BaseEntityMap
	{
		/// <summary>
		/// Reference to parent terrain dataset
		/// </summary>
		public Guid TerrainDatasetId { get; set; }

		/// <summary>
		/// OSM element ID (way or relation)
		/// </summary>
		public long OsmId { get; set; }

		/// <summary>
		/// Feature type: water, forest, wetland, cliff, desert, urban, military, road
		/// </summary>
		public string FeatureType { get; set; } = string.Empty;

		/// <summary>
		/// Sub-type (e.g., "river", "lake" for water type)
		/// </summary>
		public string? SubType { get; set; }

		/// <summary>
		/// Feature name if available
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// GeoJSON geometry (LineString or Polygon)
		/// </summary>
		public string GeometryJson { get; set; } = string.Empty;

		/// <summary>
		/// Additional OSM tags (JSON object)
		/// </summary>
		public string? TagsJson { get; set; }

		/// <summary>
		/// Bounding box: minimum latitude
		/// </summary>
		public double BBoxMinLat { get; set; }

		/// <summary>
		/// Bounding box: maximum latitude
		/// </summary>
		public double BBoxMaxLat { get; set; }

		/// <summary>
		/// Bounding box: minimum longitude
		/// </summary>
		public double BBoxMinLon { get; set; }

		/// <summary>
		/// Bounding box: maximum longitude
		/// </summary>
		public double BBoxMaxLon { get; set; }

		/// <summary>
		/// Difficulty rating for vehicles (0-10, where 10 is impassable)
		/// </summary>
		public int DifficultyRating { get; set; }
	}

	/// <summary>
	/// Stores coastline data for sea crossing detection
	/// </summary>
	public class CoastlineSegment : BaseEntityMap
	{
		/// <summary>
		/// Reference to parent terrain dataset
		/// </summary>
		public Guid TerrainDatasetId { get; set; }

		/// <summary>
		/// OSM way ID
		/// </summary>
		public long OsmId { get; set; }

		/// <summary>
		/// GeoJSON LineString of coastline
		/// </summary>
		public string GeometryJson { get; set; } = string.Empty;

		/// <summary>
		/// Bounding box for quick spatial queries
		/// </summary>
		public double BBoxMinLat { get; set; }
		public double BBoxMaxLat { get; set; }
		public double BBoxMinLon { get; set; }
		public double BBoxMaxLon { get; set; }
	}
}
