
using Microsoft.AspNetCore.Identity;
using TechWebSol.Models;
using TechWebSol.Constants;

namespace TechWebSol.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager
           )
        {
            context.Database.EnsureCreated();


            // Create default super admin user if it doesn't exist
            var adminUser = await userManager.FindByEmailAsync("SuperAdmin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "SuperAdmin",
                    Email = "SuperAdmin@web.com",
                    EmailConfirmed = true,
                    isSuperAdmin = true,
                    FirstName = "Admin",
                    LastName = "User",
                    UserCode = "ADMIN001",
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AppConstants.AdminRole);
                }
            }
        }
    }
}
