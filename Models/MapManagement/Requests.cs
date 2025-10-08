namespace TechWebSol.Models.MapManagement
{
	public record BoundsDto(double North, double South, double East, double West);
	public record PreviewRequest(BoundsDto Bounds, List<int> Zoom_Levels);
	public record DownloadRequest(BoundsDto Bounds, List<int> Zoom_Levels, string? Format, string? Map_Style);
}
