namespace TechWebSol.ViewModels
{
    public class ApplicationUserRoleVM
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        public string IsActive { get; set; } = string.Empty;
        public string HomeUrl { get; set; } = string.Empty;
        public int? LoginCount { get; set; }
    }
}
