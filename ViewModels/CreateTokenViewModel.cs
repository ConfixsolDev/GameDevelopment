using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TechWebSol.Models;

namespace TechWebSol.ViewModels
{
    /// <summary>
    /// ViewModel for creating a simple token (map representation only)
    /// Asset type and detailed specs will come from data entry layer
    /// </summary>
    public class CreateTokenViewModel
    {
        [Required(ErrorMessage = "Token name is required")]
        [StringLength(100, ErrorMessage = "Token name cannot exceed 100 characters")]
        [Display(Name = "Token Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Token Group")]
        public Guid? TokenGroupId { get; set; }

        [Display(Name = "Asset Image/Symbol")]
        public IFormFile? AssetImage { get; set; }

        [Display(Name = "Coverage Radius (km)")]
        [Range(0.1, 1000, ErrorMessage = "Coverage radius must be between 0.1 and 1000 km")]
        public decimal? CoverageRadiusKm { get; set; }

        [Display(Name = "Current Latitude")]
        public decimal? CurrentLatitude { get; set; }

        [Display(Name = "Current Longitude")]
        public decimal? CurrentLongitude { get; set; }

        // For dropdown display
        public List<TokenGroup>? AvailableTokenGroups { get; set; }

        // For form state
        public bool IsEdit { get; set; } = false;
    }
}