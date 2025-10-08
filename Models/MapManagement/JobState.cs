namespace TechWebSol.Models.MapManagement
{
	public class DownloadJob
	{
		public int Progress { get; set; }
		public int Total { get; set; }
		public bool Done { get; set; }
		public string? Error { get; set; }
		public byte[]? FileBytes { get; set; }
		public string Format { get; set; } = "zip";
		public string Style { get; set; } = "map";
		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
		public DateTime? CompletedUtc { get; set; }
		public string? FileName { get; set; }
	}
}
