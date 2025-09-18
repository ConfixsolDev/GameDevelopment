using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net.Mail;
using System.Net.Sockets;
using TechWebSol.Constants;
using TechWebSol.Extensions;
using TechWebSol.Models;
using TechWebSol.Models.DocumentModal;
using TechWebSol.ViewModels;

namespace TechWebSol.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private ISession Session => httpContextAccessor?.HttpContext?.Session;
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        
        public DbSet<AppRoles> AppRoles { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<TokenSignature> TokenSignatures { get; set; }
        public DbSet<StabilityInfo> StabilityInfo { get; set; }
        public DbSet<TouchGeometry> TouchGeometry { get; set; }
        public DbSet<TouchPattern> TouchPatterns { get; set; }
        public DbSet<MultiTouchGeometry> MultiTouchGeometry { get; set; }
        public DbSet<MapMarker> MapMarkers { get; set; }
        public DbSet<TokenGroup> TokenGroups { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamTokenGroupAssignment> TeamTokenGroupAssignments { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        
        // Military Unit DbSets
        public DbSet<InfantryBattalion> InfantryBattalions { get; set; }
        public DbSet<ArmouredRegiment> ArmouredRegiments { get; set; }
        public DbSet<ArtilleryRegiment> ArtilleryRegiments { get; set; }
        public DbSet<TerrainMobilityFactor> TerrainMobilityFactors { get; set; }
        public DbSet<ForceProtection> ForceProtections { get; set; }
        public DbSet<Brigade> Brigades { get; set; }

        // War Game Simulation DbSets
        public DbSet<WarGameScenario> WarGameScenarios { get; set; }
        public DbSet<UnitDeployment> UnitDeployments { get; set; }
        public DbSet<MovementOrder> MovementOrders { get; set; }
        public DbSet<Battle> Battles { get; set; }
        public DbSet<BattleParticipant> BattleParticipants { get; set; }
        public DbSet<CombatResult> CombatResults { get; set; }
        public DbSet<Objective> Objectives { get; set; }
        public DbSet<SimulationEvent> SimulationEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            base.OnModelCreating(modelBuilder);
            // Configure Identity tables
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.isSuperAdmin).HasDefaultValue(false);

                entity.HasOne(e => e.Team)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("AspNetRoles");
                entity.Property(e => e.ApplicationId).HasMaxLength(450);
                entity.Property(e => e.Access).HasMaxLength(4000);
            });

            // Configure IdentityUserRole, IdentityUserClaim, IdentityUserLogin, IdentityUserToken, IdentityRoleClaim
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("AspNetUserRoles");
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("AspNetUserClaims");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("AspNetUserLogins");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("AspNetUserTokens");
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("AspNetRoleClaims");
            });

            // Configure Military Unit entities
            modelBuilder.Entity<InfantryBattalion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ArmouredRegiment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ArtilleryRegiment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TerrainMobilityFactor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TerrainType).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ForceProtection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProtectionType).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Brigade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BrigadeCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Token)
                    .WithMany()
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure War Game Simulation entities
            modelBuilder.Entity<WarGameScenario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ScenarioCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.GameSession)
                    .WithMany()
                    .HasForeignKey(e => e.GameSessionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UnitDeployment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Position).IsRequired().HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Scenario)
                    .WithMany(e => e.UnitDeployments)
                    .HasForeignKey(e => e.ScenarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MovementOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartPosition).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.EndPosition).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.MovementType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.UnitDeployment)
                    .WithMany(e => e.MovementOrders)
                    .HasForeignKey(e => e.UnitDeploymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Battle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BattleName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BattleLocation).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.BattleType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.Scenario)
                    .WithMany(e => e.Battles)
                    .HasForeignKey(e => e.ScenarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BattleParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.Battle)
                    .WithMany(e => e.Participants)
                    .HasForeignKey(e => e.BattleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UnitDeployment)
                    .WithMany(e => e.BattleParticipations)
                    .HasForeignKey(e => e.UnitDeploymentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<CombatResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Result).HasMaxLength(20);

                entity.HasOne(e => e.Battle)
                    .WithMany(e => e.CombatResults)
                    .HasForeignKey(e => e.BattleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Objective>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ObjectiveName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ObjectiveType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ObjectiveLocation).IsRequired().HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Scenario)
                    .WithMany(e => e.Objectives)
                    .HasForeignKey(e => e.ScenarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SimulationEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EventData).IsRequired().HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Scenario)
                    .WithMany()
                    .HasForeignKey(e => e.ScenarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changeSet = this.ChangeTracker.Entries<BaseEntity>();
            var userDetails = Session.GetObject<ApplicationUserVM>(AppConstants.UserSessionKey);

            // Get Pakistan Standard Time zone
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            if (changeSet != null)
            {
                foreach (var entry in changeSet.Where(c => c.State != EntityState.Unchanged))
                {
                    // Convert current UTC time to Pakistan Standard Time
                    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);

                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.Id = Guid.NewGuid();
                        entry.Entity.CreatedBy = userDetails.ApplicationUserId;
                        entry.Entity.CreatedDate = now;
                        entry.Entity.UpdatedBy = userDetails.ApplicationUserId;
                        entry.Entity.UpdatedDate = now;
                        entry.Entity.TeamId = userDetails.TeamId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        var original = await GenerateOriginalEntityAsync<BaseEntity>(entry.GetDatabaseValues());
                        entry.Entity.UpdatedBy = userDetails.ApplicationUserId;
                        entry.Entity.UpdatedDate = now;

                        entry.Entity.CreatedDate = original.CreatedDate;
                        entry.Entity.CreatedBy = original.CreatedBy;
                        entry.Entity.TeamId = original.TeamId;
                    }
                }
            }
            return await base.SaveChangesAsync();
        }

        private async Task<T> GenerateOriginalEntityAsync<T>(PropertyValues values) where T : new()
        {
            return await Task.Run(() =>
            {
                T entity = new T();
                Type type = typeof(T);
                var baseProperties = type.GetProperties();
                foreach (var property in baseProperties)
                {
                    property.SetValue(entity, values[property.Name]);
                }
                return entity;
            });
        }
    }
}
