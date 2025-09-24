using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechWebSol.Constants;
using TechWebSol.Models;
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

            await context.Database.MigrateAsync().ConfigureAwait(false);

            if (!await context.Teams.AnyAsync())
            {
                var teamFox = new Team
                {
                    Name = "Fox Land",
                    Description = "This is the default team.",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var teamBlue = new Team
                {
                    Name = "Blue Land",
                    Description = "This is the default team.",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                context.Teams.Add(teamFox);
                context.Teams.Add(teamBlue);
                await context.SaveChangesAsync();
            }

            var mvcControllers = _mvcControllerDiscovery.GetControllers();
            foreach (var controller in mvcControllers)
            {
                foreach (var action in controller.Actions)
                {
                    action.ControllerId = controller.Id;
                }
            }
            var fullAccessJson = JsonConvert.SerializeObject(mvcControllers);

            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            async Task EnsureRoleAsync(string roleName, string? accessJson = null)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        ApplicationId = AppConstants.Id,
                        Access = accessJson ?? "[]"
                    };

                    var createResult = await roleManager.CreateAsync(role);
                    if (!createResult.Succeeded)
                    {
                        var reasons = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Failed to create role '{roleName}': {reasons}");
                    }
                }
            }

            await EnsureRoleAsync("Super Administrator", fullAccessJson);
            await EnsureRoleAsync("Control", fullAccessJson);
            await EnsureRoleAsync("Director", fullAccessJson);
            await EnsureRoleAsync("Fox Land", fullAccessJson);
            await EnsureRoleAsync("Blue Land", fullAccessJson);

            if (!await context.AppRoles.AnyAsync(ar => ar.AppId == AppConstants.Id))
            {
                var appRole = new AppRoles
                {
                    AppId = AppConstants.Id,
                    RoleAccess = fullAccessJson
                };
                context.AppRoles.Add(appRole);
                await context.SaveChangesAsync();
            }

            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var defaultPassword = "Admin123!";

            async Task EnsureUserAsync(string userName, string email, string firstName, string lastName, string roleName,string homeUrl)
            {
                var existingUser = await userManager.FindByNameAsync(userName);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = true,
                        HomeUrl = homeUrl 
                    };

                    var result = await userManager.CreateAsync(user, defaultPassword);
                    if (!result.Succeeded)
                    {
                        var reasons = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Failed to create user '{userName}': {reasons}");
                    }

                    await userManager.AddToRoleAsync(user, roleName);
                }
            }

            await EnsureUserAsync("superadmin", "superadmin@example.com", "Super", "Admin", "Super Administrator", "/Home/Index");
            await EnsureUserAsync("control", "control@example.com", "Control", "User", "Control", "/GamePlay/Index");
            await EnsureUserAsync("director", "director@example.com", "Director", "User", "Director", "/GamePlay/Index");
            await EnsureUserAsync("foxland", "foxland@example.com", "Fox", "Land", "Fox Land", "/GamePlay/Index");
            await EnsureUserAsync("blueland", "blueland@example.com", "Blue", "Land", "Blue Land", "/GamePlay/Index");
        }
    }
}
