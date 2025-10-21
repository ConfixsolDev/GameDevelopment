namespace TechWebSol.DTOs
{
    public class TokenListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? TokenGroupId { get; set; }
        public string? TokenGroupName { get; set; }
        public bool IsActive { get; set; }
        public bool IsManualToken { get; set; }
        public DateTime? LastUsed { get; set; }
        public int? UsageCount { get; set; }
        public string? Notes { get; set; }
        public string? AssetImagePath { get; set; }
        public decimal? FrontCoverageKm { get; set; }
        public decimal? RearCoverageKm { get; set; }
        public decimal? SideCoverageKm { get; set; }
        public Guid? TeamId { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public decimal? CoverageRadiusKm { get; set; }
    }
}
