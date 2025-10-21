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

            // ---- Team Types and Teams -----------------------------------------
            await SeedTeamDataAsync(context);

            // ---- Military Data Seeding -----------------------------------------
            await SeedMilitaryDataAsync(context);

            // ---- Assign Users to Teams (at the end) ---------------------------
            await AssignUsersToTeamsAsync(context);
        }

        // ---- Team Data Seeding Method -----------------------------------------
        private static async Task SeedTeamDataAsync(ApplicationDbContext context)
        {
            try
            {
                Console.WriteLine("Starting team data seeding...");

                // Create Blue Force Team
                var blueTeam = new Team
                {
                    Id = Guid.NewGuid(),
                    Name = "Blue Force Team",
                    TeamCode = "BLUE-FORCE",
                    Description = "Blue B Field Operating System Team",
                    Category = "Military Force",
                    ForceType = "Blue",
                    CreatedBy = "SystemSeeder",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = "SystemSeeder",
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                // Create Red Force Team
                var redTeam = new Team
                {
                    Id = Guid.NewGuid(),
                    Name = "Red Force Team",
                    TeamCode = "RED-FORCE",
                    Description = "Red B Field Operating System Team",
                    Category = "Military Force",
                    ForceType = "Red",
                    CreatedBy = "SystemSeeder",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = "SystemSeeder",
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                context.Teams.Add(blueTeam);
                context.Teams.Add(redTeam);
                await context.SaveChangesAsync();

                Console.WriteLine("Team data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during team data seeding: {ex.Message}");
                // Don't throw - seeding failures shouldn't crash the application startup
            }
        }

        // ---- User Team Assignment Method -------------------------------------
        private static async Task AssignUsersToTeamsAsync(ApplicationDbContext context)
        {
            try
            {
                Console.WriteLine("Assigning users to teams...");

                // Get teams
                var blueTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "BLUE-FORCE");
                var redTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "RED-FORCE");

                if (blueTeam == null || redTeam == null)
                {
                    Console.WriteLine("Teams not found. Cannot assign users to teams.");
                    return;
                }

                // Get users
                var bluelandUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "blueland");
                var foxlandUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "foxland");

                if (bluelandUser != null)
                {
                    bluelandUser.TeamId = blueTeam.Id;
                     context.Users.Update(bluelandUser);
                    Console.WriteLine($"Assigned blueland user to Blue Force Team");
                }

                if (foxlandUser != null)
                {
                    foxlandUser.TeamId = redTeam.Id;
                    context.Users.Update(foxlandUser);
                    Console.WriteLine($"Assigned foxland user to Red Force Team");
                }

                await context.SaveChangesAsync();
                Console.WriteLine("User team assignments completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during user team assignment: {ex.Message}");
                // Don't throw - seeding failures shouldn't crash the application startup
            }
        }

        // ---- Military Data Seeding Method -------------------------------------
        private static async Task SeedMilitaryDataAsync(ApplicationDbContext context)
        {
            try
            {
                Console.WriteLine("Starting military data insertion...");

                // Get team IDs for assignment
                var blueTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "BLUE-FORCE");
                var redTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "RED-FORCE");

                if (blueTeam == null || redTeam == null)
                {
                    Console.WriteLine("Teams not found. Please ensure team seeding completed successfully.");
                    return;
                }

                // Create Blue and Red Brigades first
                var blueBrigadeId = await CreateBrigadeAsync(context, "Blue B Field Operating System", "Blue", blueTeam.Id);
                var redBrigadeId = await CreateBrigadeAsync(context, "Red B Field Operating System", "Red", redTeam.Id);

                // Insert Infantry Battalions
                await InsertInfantryBattalionsAsync(context, blueBrigadeId, redBrigadeId);

                // Insert Armoured Regiments
                await InsertArmouredRegimentsAsync(context, blueBrigadeId, redBrigadeId);

                // Insert Artillery Regiments
                await InsertArtilleryRegimentsAsync(context, blueBrigadeId, redBrigadeId);

                // Create tokens for all military units
                await CreateTokensForMilitaryUnitsAsync(context);

                await context.SaveChangesAsync();
                Console.WriteLine("All military data inserted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting military data: {ex.Message}");
                // Don't throw - seeding failures shouldn't crash the application startup
            }
        }

        private static async Task<Guid> CreateBrigadeAsync(ApplicationDbContext context, string name, string forceType, Guid teamId)
        {
            var brigade = new Brigade
            {
                Id = Guid.NewGuid(),
                BrigadeCode = forceType == "Blue" ? "BLUE-B-FOS" : "RED-B-FOS",
                ForceType = forceType,
                TeamId = teamId,
                CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "SystemSeeder",
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            context.Brigades.Add(brigade);
            await context.SaveChangesAsync();

            Console.WriteLine($"Created {forceType} Brigade: {name}");
            return brigade.Id;
        }

        private static async Task InsertInfantryBattalionsAsync(ApplicationDbContext context, Guid blueBrigadeId, Guid redBrigadeId)
        {
            // Get team IDs
            var blueTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "BLUE-FORCE");
            var redTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "RED-FORCE");

            // Blue B Infantry Battalion (Strength 765)
            var blueInfantry = new InfantryBattalion
            {
                Id = Guid.NewGuid(),
                Name = "Blue B Infantry Battalion",
                Description = "Blue B Field Operating System Infantry Battalion",
                UnitCode = "BLUE-INF-BN-001",
                Strength = 765,
                ForceType = "Blue",
                UnitType = "Infantry",
                BrigadeId = blueBrigadeId,
                TokenId = null, // Will be set when token is created
                TeamId = blueTeam?.Id, // Assign Blue team
                Companies = 5, // 4+1 companies
                ATGMS = 8,
                RocketLauncher = 14,
                Mortars81mm = 8, // Split: 8x 81mm mortars
                Mortars120mm = 4, // Split: 4x 120mm mortars
                GrenadeLaunchers = 4,
                HMG_AGL = 8,
                MG_LMG = 62,
                MANPADS = 0,
                Grenades = 818,
                Drones = 0,
                DroneTypes = "",
                MarchingSpeedTrucksRoads = 30m, // kmph - decimal type
                MarchingSpeedAPCs = 20m, // kmph - decimal type
                MarchingSpeedCrossCountry = 5m, // kmph - decimal type
                MarchingSpeedAPCsCrossCountry = 10m, // kmph - decimal type
                CombatAdvanceSpeed = 1m, // kmph average - decimal type
                CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "SystemSeeder",
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Red B Infantry Battalion (Strength 730)
            var redInfantry = new InfantryBattalion
            {
                Id = Guid.NewGuid(),
                Name = "Red B Infantry Battalion",
                Description = "Red B Field Operating System Infantry Battalion",
                UnitCode = "RED-INF-BN-001",
                Strength = 730,
                ForceType = "Red",
                UnitType = "Infantry",
                BrigadeId = redBrigadeId,
                TokenId = null, // Will be set when token is created
                TeamId = redTeam?.Id, // Assign Red team
                Companies = 4, // 3+1 companies
                ATGMS = 12,
                RocketLauncher = 20,
                Mortars81mm = 12, // Split: 12x 81mm mortars
                Mortars120mm = 8, // Split: 8x 120mm mortars
                GrenadeLaunchers = 6,
                HMG_AGL = 12,
                MG_LMG = 62,
                MANPADS = 2,
                Grenades = 800,
                Drones = 0,
                DroneTypes = "",
                MarchingSpeedTrucksRoads = 30m, // kmph - decimal type
                MarchingSpeedAPCs = 20m, // kmph - decimal type
                MarchingSpeedCrossCountry = 5m, // kmph - decimal type
                MarchingSpeedAPCsCrossCountry = 10m, // kmph - decimal type
                CombatAdvanceSpeed = 1m, // kmph average - decimal type
                CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "SystemSeeder",
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            context.InfantryBattalions.Add(blueInfantry);
            context.InfantryBattalions.Add(redInfantry);

            Console.WriteLine("Infantry Battalions added to context");
        }

        private static async Task InsertArmouredRegimentsAsync(ApplicationDbContext context, Guid blueBrigadeId, Guid redBrigadeId)
        {
            // Get team IDs
            var blueTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "BLUE-FORCE");
            var redTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "RED-FORCE");

            // Armour Regiment (Strength 530)
            var armourRegiment = new ArmouredRegiment
            {
                Id = Guid.NewGuid(),
                Name = "Armour Regiment",
                Description = "Armour Regiment - Blue Force",
                UnitCode = "BLUE-ARM-REG-001",
                Strength = 530,
                ForceType = "Blue",
                UnitType = "Armoured",
                BrigadeId = blueBrigadeId,
                TokenId = null, // Will be set when token is created
                TeamId = blueTeam?.Id, // Assign Blue team
                Squadrons = 4, // 3+1 squadrons
                Tanks = 44,
                ATGMS = 6,
                Mortars120mm = 12,
                HMG = 6,
                Drones = 0,
                DroneTypes = "",
                MarchingSpeedRoads = 15m, // kmph - decimal type
                MarchingSpeedCrossCountry = 10m, // kmph - decimal type
                CombatAdvanceSpeed = 1m, // kmph average - decimal type
                CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "SystemSeeder",
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Tank Regiment (Strength 5080) - Corrected from 508
            var tankRegiment = new ArmouredRegiment
            {
                Id = Guid.NewGuid(),
                Name = "Tank Regiment",
                Description = "Tank Regiment - Red Force",
                UnitCode = "RED-TANK-REG-001",
                Strength = 5080, // Corrected from 508 to match source document
                ForceType = "Red",
                UnitType = "Armoured",
                BrigadeId = redBrigadeId,
                TokenId = null, // Will be set when token is created
                TeamId = redTeam?.Id, // Assign Red team
                Squadrons = 5, // 4+1 squadrons
                Tanks = 58,
                ATGMS = 6,
                Mortars120mm = 4,
                HMG = 6,
                Drones = 0,
                DroneTypes = "",
                MarchingSpeedRoads = 15m, // kmph - decimal type
                MarchingSpeedCrossCountry = 10m, // kmph - decimal type
                CombatAdvanceSpeed = 1m, // kmph average - decimal type
                CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "SystemSeeder",
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            context.ArmouredRegiments.Add(armourRegiment);
            context.ArmouredRegiments.Add(tankRegiment);

            Console.WriteLine("Armoured Regiments added to context");
        }

        private static async Task InsertArtilleryRegimentsAsync(ApplicationDbContext context, Guid blueBrigadeId, Guid redBrigadeId)
        {
            // Get team IDs
            var blueTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "BLUE-FORCE");
            var redTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "RED-FORCE");

            // Artillery Regiment (155 mm SP) (Strength 520)
            var artilleryRegiment = new ArtilleryRegiment
            {
                Id = Guid.NewGuid(),
                Name = "Artillery Regiment (155mm SP)",
                Description = "Artillery Regiment - Blue Force",
                UnitCode = "BLUE-ART-REG-001",
                Strength = 520,
                ForceType = "Blue",
                UnitType = "Artillery",
                BrigadeId = blueBrigadeId,
                TokenId = null, // Will be set when token is created
                TeamId = blueTeam?.Id, // Assign Blue team
                Batteries = 4, // 3+1 batteries
                Guns = 18,
                GunRange = 18m, // km - decimal type
                GunCaliber = "155mm SP",
                HMG = 18,
                Drones = 0,
                DroneTypes = "",
                CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "SystemSeeder",
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Artillery Battalion (155 mm SP) (Strength 500)
            var artilleryBattalion = new ArtilleryRegiment
            {
                Id = Guid.NewGuid(),
                Name = "Artillery Battalion (155mm SP)",
                Description = "Artillery Battalion - Red Force",
                UnitCode = "RED-ART-BN-001",
                Strength = 500,
                ForceType = "Red",
                UnitType = "Artillery",
                BrigadeId = redBrigadeId,
                TokenId = null, // Will be set when token is created
                TeamId = redTeam?.Id, // Assign Red team
                Batteries = 4, // 3+1 batteries
                Guns = 18,
                GunRange = 29m, // km - decimal type
                GunCaliber = "155mm SP",
                HMG = 18,
                Drones = 0,
                DroneTypes = "",
                CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "SystemSeeder",
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            context.ArtilleryRegiments.Add(artilleryRegiment);
            context.ArtilleryRegiments.Add(artilleryBattalion);

            Console.WriteLine("Artillery Regiments added to context");
        }

        private static async Task CreateTokensForMilitaryUnitsAsync(ApplicationDbContext context)
        {
            try
            {
                Console.WriteLine("Creating tokens for teams...");

                // Get teams
                var blueTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "BLUE-FORCE");
                var redTeam = await context.Teams.FirstOrDefaultAsync(t => t.TeamCode == "RED-FORCE");

                if (blueTeam == null || redTeam == null)
                {
                    Console.WriteLine("Teams not found. Cannot create tokens.");
                    return;
                }

                int tokenCount = 0;

                // Create 7 tokens for Blue Force Team
                for (int i = 1; i <= 7; i++)
                {
                    var blueToken = new Token
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Blue Force Token {i}",
                        ForceType = "Blue",
                        TeamId = blueTeam.Id,
                        TrainingConsistency = 85.0m,
                        IsManualToken = false,
                        LastUsed = DateTime.UtcNow,
                        UsageCount = 0,
                        Notes = $"Blue Force Team Token {i}",
                        AssetImagePath = "/assets/tokens/blue-force.png",
                        FrontCoverageKm = 3.0m,
                        RearCoverageKm = 2.0m,
                        SideCoverageKm = 2.5m,
                        OrganizationLevel = Constants.OrganizationLevel.Brigade,
                        UnitType = Constants.UnitType.Infantry,
                        UnitDesignation = $"BLUE-T{i:D2}",
                        CreatedBy = "SystemSeeder",
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = "SystemSeeder",
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    context.Tokens.Add(blueToken);
                    tokenCount++;
                }

                // Create 7 tokens for Red Force Team
                for (int i = 1; i <= 7; i++)
                {
                    var redToken = new Token
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Red Force Token {i}",
                        ForceType = "Red",
                        TeamId = redTeam.Id,
                        TrainingConsistency = 85.0m,
                        IsManualToken = false,
                        LastUsed = DateTime.UtcNow,
                        UsageCount = 0,
                        Notes = $"Red Force Team Token {i}",
                        AssetImagePath = "/assets/tokens/red-force.png",
                        FrontCoverageKm = 3.0m,
                        RearCoverageKm = 2.0m,
                        SideCoverageKm = 2.5m,
                        OrganizationLevel = Constants.OrganizationLevel.Brigade,
                        UnitType = Constants.UnitType.Infantry,
                        UnitDesignation = $"RED-T{i:D2}",
                        CreatedBy = "SystemSeeder",
                CreatedDate = DateTime.UtcNow,
                        UpdatedBy = "SystemSeeder",
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    context.Tokens.Add(redToken);
                    tokenCount++;
                }

                Console.WriteLine($"Created {tokenCount} tokens (7 for Blue Force Team, 7 for Red Force Team)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tokens: {ex.Message}");
                throw;
            }
        }

    }
}
