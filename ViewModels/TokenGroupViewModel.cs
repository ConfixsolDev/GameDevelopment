using System.ComponentModel.DataAnnotations;
using TechWebSol.Models;

namespace TechWebSol.ViewModels
{
    /// <summary>
    /// ViewModel for token group listing and management
    /// </summary>
    public class TokenGroupViewModel
    {
        public Guid Id { get; set; }
        
        [Display(Name = "Group Name")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Group Code")]
        public string GroupCode { get; set; } = string.Empty;
        
        [Display(Name = "Category")]
        public string? Category { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Created By")]
        public string? CreatedByUserName { get; set; }
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Token Count")]
        public int TokenCount { get; set; }
        
        [Display(Name = "Team Assignments")]
        public int TeamAssignmentCount { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a new token group
    /// </summary>
    public class CreateTokenGroupViewModel
    {
        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
        [Display(Name = "Group Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Group code is required")]
        [StringLength(50, ErrorMessage = "Group code cannot exceed 50 characters")]
        [Display(Name = "Group Code")]
        public string GroupCode { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel for editing an existing token group
    /// </summary>
    public class EditTokenGroupViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters")]
        [Display(Name = "Group Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Group code is required")]
        [StringLength(50, ErrorMessage = "Group code cannot exceed 50 characters")]
        [Display(Name = "Group Code")]
        public string GroupCode { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel for token group details
    /// </summary>
    public class TokenGroupDetailsViewModel
    {
        public Guid Id { get; set; }
        
        [Display(Name = "Group Name")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Group Code")]
        public string GroupCode { get; set; } = string.Empty;
        
        [Display(Name = "Category")]
        public string? Category { get; set; }
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Created By")]
        public string? CreatedByUserName { get; set; }
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Token Count")]
        public int TokenCount { get; set; }
        
        [Display(Name = "Team Assignments")]
        public int TeamAssignmentCount { get; set; }
        
        public List<Token> Tokens { get; set; } = new List<Token>();
        public List<TeamTokenGroupAssignment> TeamAssignments { get; set; } = new List<TeamTokenGroupAssignment>();
    }

    /// <summary>
    /// ViewModel for token group index page
    /// </summary>
    public class TokenGroupIndexViewModel
    {
        public List<TokenGroupViewModel> TokenGroups { get; set; } = new List<TokenGroupViewModel>();
        public string? SearchTerm { get; set; }
        public string? CategoryFilter { get; set; }
        public bool? StatusFilter { get; set; }
        public List<string> AvailableCategories { get; set; } = new List<string>();
    }
}
