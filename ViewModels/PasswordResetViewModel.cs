using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TechWebSol.ViewModels
{
    public class PasswordResetViewModel
    {
        [Required]
        [DisplayName("Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DisplayName("Application User ID")]
        public string ApplilicationUserId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Old Password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("New Password")]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Confirm New Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
