namespace TechWebSol.Models
{
    public class MapStorageSettings
    {
        public string BasePath { get; set; } = "wwwroot/maps";
        public int MaxSizeGB { get; set; } = 50;
        public int CleanupOldMapsAfterDays { get; set; } = 90;
        public List<string> AllowedFormats { get; set; } = new() { "mbtiles" };
        public bool UseTileServerGL { get; set; } = true;
        public int TileServerPort { get; set; } = 8080;
    }
}

