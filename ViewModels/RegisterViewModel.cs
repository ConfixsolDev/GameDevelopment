using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWebSol.ViewModels
{
    public class RegisterViewModel
    {
        public string ApplicationUserID { get; set; } = string.Empty;

        [Required]
        [DisplayName("Username")]
        public string UserName { get; set; } = string.Empty;
        public string AvatarURL { get; set; } = string.Empty;

        [DisplayName("Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [DisplayName("Profile Image")]
        public string ProfileImage { get; set; } = string.Empty;

        [DisplayName("Team")]
        public Guid? TeamId { get; set; }

        [DisplayName("Force Type")]
        [Required(ErrorMessage = "Force Type is required")]
        public string ForceType { get; set; } = string.Empty;

        [MaxLength(128)]
        [NotMapped]
        public string RoleName { get; set; } = string.Empty;

        public List<string> Role { get; set; } = new List<string>();

        public List<SelectListItem> TeamList { get; set; } = new List<SelectListItem>();

        public bool IsOnline { get; set; }
        public DateTime? LastLoginDate { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;

        public int LoginCount { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        public string FullName => $"{this.FirstName} {this.LastName}".Trim();

        [DataType(DataType.Password)]
        [DisplayName("Password")]
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [DisplayName("Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Usercode { get; set; } = string.Empty;

        [DisplayName("Email")]
        public string UserEmail { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();

    }
}
