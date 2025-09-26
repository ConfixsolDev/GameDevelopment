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
            using var serviceScope = _serviceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Apply migrations
            await context.Database.MigrateAsync().ConfigureAwait(false);

            // ---- TeamTypes (ensure & get IDs) ---------------------------------
            var foxType = await EnsureTeamTypeAsync(context, "Fox Land", "This is the default team.", "fox-land");
            var blueType = await EnsureTeamTypeAsync(context, "Blue Land", "This is the default team.", "blue-land");

            // ---- Teams (ensure) -----------------------------------------------
            await EnsureTeamAsync(context, "Team Fox Land", "This is the default team.", foxType.Id);
            await EnsureTeamAsync(context, "Team Blue Land", "This is the default team.", blueType.Id);

            // ---- Discover controllers/actions & serialize full access ----------
            var mvcControllers = _mvcControllerDiscovery.GetControllers();
            foreach (var controller in mvcControllers)
            {
                foreach (var action in controller.Actions)
                {
                    action.ControllerId = controller.Id;
                }
            }
            var fullAccessJson = JsonConvert.SerializeObject(mvcControllers);

            // ---- Roles ---------------------------------------------------------
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            async Task EnsureRoleAsync(string roleName, string? accessJson = null)
            {
                if (!await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        ApplicationId = AppConstants.Id,
                        Access = accessJson ?? "[]"
                    };

                    var createResult = await roleManager.CreateAsync(role).ConfigureAwait(false);
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

            // ---- AppRoles (single row per AppId) -------------------------------
            if (!await context.AppRoles.AnyAsync(ar => ar.AppId == AppConstants.Id).ConfigureAwait(false))
            {
                var appRole = new AppRoles
                {
                    AppId = AppConstants.Id,
                    RoleAccess = fullAccessJson
                };
                context.AppRoles.Add(appRole);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            // ---- Users ---------------------------------------------------------
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            const string defaultPassword = "Admin123!";

            async Task EnsureUserAsync(string userName, string email, string firstName, string lastName, string roleName, string homeUrl)
            {
                var existingUser = await userManager.FindByNameAsync(userName).ConfigureAwait(false);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = true,
                        HomeUrl = homeUrl,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, defaultPassword).ConfigureAwait(false);
                    if (!result.Succeeded)
                    {
                        var reasons = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Failed to create user '{userName}': {reasons}");
                    }

                    var roleResult = await userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);
                    if (!roleResult.Succeeded)
                    {
                        var reasons = string.Join("; ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Failed to add '{userName}' to role '{roleName}': {reasons}");
                    }
                }
            }

            await EnsureUserAsync("superadmin", "superadmin@example.com", "Super", "Admin", "Super Administrator", "/Home/Index");
            await EnsureUserAsync("control", "control@example.com", "Control", "User", "Control", "/GamePlay/Index");
            await EnsureUserAsync("director", "director@example.com", "Director", "User", "Director", "/GamePlay/Index");
            await EnsureUserAsync("foxland", "foxland@example.com", "Fox", "Land", "Fox Land", "/GamePlay/Index");
            await EnsureUserAsync("blueland", "blueland@example.com", "Blue", "Land", "Blue Land", "/GamePlay/Index");
        }

        // -------------------- Helpers --------------------

        private static async Task<TeamType> EnsureTeamTypeAsync(ApplicationDbContext context, string name, string description, string typeCode)
        {
            var existing = await context.TeamTypes.FirstOrDefaultAsync(t => t.Name == name).ConfigureAwait(false);
            if (existing != null) return existing;

            var entity = new TeamType
            {
                Name = name,
                Description = description,
                TeamTypeCode = typeCode,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            context.TeamTypes.Add(entity);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return entity;
        }

        private static async Task EnsureTeamAsync(ApplicationDbContext context, string name, string description, Guid teamTypeId)
        {
            var exists = await context.Teams.AnyAsync(t => t.Name == name).ConfigureAwait(false);
            if (exists) return;

            var team = new Team
            {
                Name = name,
                Description = description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                TeamTypeId = teamTypeId
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
