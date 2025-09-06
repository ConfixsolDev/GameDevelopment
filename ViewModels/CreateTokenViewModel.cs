using System.ComponentModel.DataAnnotations;
using TechWebSol.Models;

namespace TechWebSol.ViewModels
{
    /// <summary>
    /// ViewModel for creating a new token
    /// </summary>
    public class CreateTokenViewModel
    {
        [Required(ErrorMessage = "Token name is required")]
        [StringLength(100, ErrorMessage = "Token name cannot exceed 100 characters")]
        [Display(Name = "Token Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [Display(Name = "Token Group")]
        public Guid? TokenGroupId { get; set; }

        // For dropdown display
        public List<TokenGroup>? AvailableTokenGroups { get; set; }

        // For form state
        public bool IsEdit { get; set; } = false;
    }
}
