using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models.Map
{
    public class MapDocument
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = "Default Map";

        // Store raw GeoJSON text
        public string RegionsJson { get; set; } = "{\"type\":\"FeatureCollection\",\"features\":[]}";
        public string ObstaclesJson { get; set; } = "{\"type\":\"FeatureCollection\",\"features\":[]}";
        public string SafeJson { get; set; } = "{\"type\":\"FeatureCollection\",\"features\":[]}";

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    }
}
