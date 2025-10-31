namespace TechWebSol.Models.MapManagement
{
	public class DownloadJob
	{
		public int Progress { get; set; }
		public int Total { get; set; }
		public bool Done { get; set; }
		public string? Error { get; set; }
		public byte[]? FileBytes { get; set; }
		public string Format { get; set; } = "mbtiles";
		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
		public DateTime? CompletedUtc { get; set; }
		public string? FileName { get; set; }  // Will store folder path
		public string? StreetFileName { get; set; }  // Street MBTiles file
		public string? SatelliteFileName { get; set; }  // Satellite MBTiles file
		
		// Terrain data properties for offline tactical analysis
		public string? TerrainDataFileName { get; set; }
		public string? TerrainDataError { get; set; }
	}
}
