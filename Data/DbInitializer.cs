using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Models;
using TechWebSol.Constants;
using Newtonsoft.Json;
using TechWebSol.Services;

namespace TechWebSol.Data
{
    public class DbInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMvcControllerDiscovery _mvcControllerDiscovery;

        public DbInitializer(IServiceProvider serviceProvider, IMvcControllerDiscovery mvcControllerDiscovery)
        {
            _serviceProvider = serviceProvider;
            _mvcControllerDiscovery = mvcControllerDiscovery;
        }

        public async Task InitializeAsync()
        {
            using var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply pending migrations
            await context.Database.MigrateAsync().ConfigureAwait(false);

            // Seed data only if there are no users yet
            if (!await context.Users.AnyAsync())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                // Create the "Super Administrator" role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Super Administrator"))
                {
                    var mvcControllers = _mvcControllerDiscovery.GetControllers();

                    // Set the controller ID on each action so the JSON is complete
                    foreach (var controller in mvcControllers)
                    {
                        foreach (var action in controller.Actions)
                        {
                            action.ControllerId = controller.Id;
                        }
                    }

                    var role = new ApplicationRole
                    {
                        Name = "Super Administrator",
                        ApplicationId=AppConstants.Id,
                        Access = JsonConvert.SerializeObject(mvcControllers)
                    };
                    try
                    {
                        await roleManager.CreateAsync(role);
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                   

                    var appRole = new AppRoles
                    {
                        AppId = AppConstants.Id,
                        RoleAccess = JsonConvert.SerializeObject(mvcControllers)
                    };
                    context.AppRoles.Add(appRole);
                    context.SaveChanges();
                }

                // Create the default administrator user
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var adminUser = new ApplicationUser
                {
                    UserName = "superadmin",
                    Email = "superadmin@example.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    IsActive = true,
                    HomeUrl= "/Home/Index"
                };

                var password = "Admin123!";
                var result = await userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Super Administrator");
                }
              
            }
           
        }
    }
}
