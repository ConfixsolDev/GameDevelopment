namespace TechWebSol.ViewModels
{
    public class ApplicationUserVM
    {
        public string ApplicationUserId { get; set; }
        
        // User Display Details 
        public string UserCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string DashBoard { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public string HomeURL { get; set; } = string.Empty;
        public Guid? TeamId { get; set; }
        public bool ApplicationRole { get; set; } = false;
        public string ForceType { get; set; } = string.Empty; // "Blueland", "Foxland", "Control"
    }

    public class ApplicationUserShort
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
    }
}
