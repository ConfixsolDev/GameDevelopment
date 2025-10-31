using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.Models
{
    [Table("DownloadedMaps")]
    public class DownloadedMap : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // User-friendly name
        
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty; // Relative path from base: maps/map-name/map.mbtiles
        
        [Required]
        public long FileSizeBytes { get; set; }
        
        public string FileSizeFriendly { get; set; } = string.Empty; // "125.5 MB"
        
        [MaxLength(50)]
        public string Format { get; set; } = "mbtiles"; // mbtiles, zip
        
        [MaxLength(50)]
        public string Style { get; set; } = string.Empty; // satellite, street, political
        
        public int MinZoom { get; set; }
        public int MaxZoom { get; set; }
        
        // Geographic bounds
        public double NorthBound { get; set; }
        public double SouthBound { get; set; }
        public double EastBound { get; set; }
        public double WestBound { get; set; }
        
        public int TileCount { get; set; }
        
        [MaxLength(500)]
        public string? TerrainDataPath { get; set; } // Path to terrain.db if exists
        
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastAccessedAt { get; set; }
        
        public int AccessCount { get; set; } = 0;
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        public bool IsFavorite { get; set; } = false;
        
        public bool IsDefault { get; set; } = false; // Only one map can be default per team
        
        [MaxLength(100)]
        public string? DownloadJobId { get; set; } // Link to original job
    }
}

