using System.ComponentModel.DataAnnotations;

namespace TechWebSol.DTOs
{
    public class CreateOrUpdateTokenRequest
    {
        public Guid? Id { get; set; }
        
        [Required(ErrorMessage = "Token name is required")]
        [StringLength(100, ErrorMessage = "Token name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        public Guid? TokenGroupId { get; set; }
        
        public bool IsManualToken { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
        
        public decimal? FrontCoverageKm { get; set; }
        public decimal? RearCoverageKm { get; set; }
        public decimal? SideCoverageKm { get; set; }
        
        // Military Unit Classification
        public string? OrganizationLevel { get; set; }
        public string? UnitType { get; set; }
        public string? UnitDesignation { get; set; }
        public string? ForceType { get; set; }
        
        // Position
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        
        // Asset
        public string? AssetImagePath { get; set; }
    }
}
