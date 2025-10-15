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
using TechWebSol.Models.Map;
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
        public DbSet<TeamType> TeamTypes { get; set; }
        public DbSet<TeamTokenGroupAssignment> TeamTokenGroupAssignments { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }

        // Military Unit DbSets
        public DbSet<InfantryBattalion> InfantryBattalions { get; set; }
        public DbSet<ArmouredRegiment> ArmouredRegiments { get; set; }
        public DbSet<ArtilleryRegiment> ArtilleryRegiments { get; set; }
        public DbSet<LogisticsUnit> LogisticsUnits { get; set; }
        public DbSet<CombatEngineeringCompany> CombatEngineeringCompanies { get; set; }
        public DbSet<TerrainMobilityFactor> TerrainMobilityFactors { get; set; }
        public DbSet<ForceProtection> ForceProtections { get; set; }
        public DbSet<Brigade> Brigades { get; set; }
        
        // Token Area Coverage
        public DbSet<TokenAreaCoverage> TokenAreaCoverages { get; set; }
        public DbSet<Intelligence> Intelligence { get; set; }
        public DbSet<Recon> Recon { get; set; }
        
        // Suspected Tokens & ISR Missions (Fog of War Intelligence)
        public DbSet<SuspectedToken> SuspectedTokens { get; set; }
        public DbSet<ISRMission> ISRMissions { get; set; }
        public DbSet<AttackOrder> AttackOrders { get; set; }

        // Attack Planning Models
        public DbSet<Models.AttackPlanning.EnhancedAttackOrder> EnhancedAttackOrders { get; set; }
        public DbSet<Models.AttackPlanning.AttackIntent> AttackIntents { get; set; }
        public DbSet<Models.AttackPlanning.AttackTiming> AttackTimings { get; set; }
        public DbSet<Models.AttackPlanning.AttackMovement> AttackMovements { get; set; }
        public DbSet<Models.AttackPlanning.FiresSupport> FiresSupports { get; set; }
        public DbSet<Models.AttackPlanning.FogOfWar> FogOfWars { get; set; }
        public DbSet<Models.AttackPlanning.AttackLogistics> AttackLogistics { get; set; }
        public DbSet<Models.AttackPlanning.RulesOfEngagement> RulesOfEngagements { get; set; }
        
        // Defense Planning Models
        public DbSet<DefenseElement> DefenseElements { get; set; }
        
        // Phase 01 Models
        public DbSet<TerrainType> TerrainTypes { get; set; }
        public DbSet<RoutesDraft> RoutesDrafts { get; set; }

        // Map Data DbSets
        public DbSet<MapRegion> MapRegions { get; set; }
        public DbSet<MapSector> MapSectors { get; set; }
        public DbSet<MapLabel> MapLabels { get; set; }
        public DbSet<MapConfiguration> MapConfigurations { get; set; }

        // War Game Simulation DbSets
        public DbSet<WarGameScenario> WarGameScenarios { get; set; }
        //public DbSet<UnitDeployment> UnitDeployments { get; set; }
        public DbSet<MovementOrder> MovementOrders { get; set; }
        public DbSet<Battle> Battles { get; set; }
        public DbSet<BattleParticipant> BattleParticipants { get; set; }
        public DbSet<CombatResult> CombatResults { get; set; }
        public DbSet<Objective> Objectives { get; set; }
        public DbSet<SimulationEvent> SimulationEvents { get; set; }

        public DbSet<MapDocument> MapDocuments { get; set; }

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

                entity.HasOne(e => e.Brigade)
                    .WithMany()
                    .HasForeignKey(e => e.BrigadeId)
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

                entity.HasOne(e => e.Brigade)
                    .WithMany()
                    .HasForeignKey(e => e.BrigadeId)
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

                entity.HasOne(e => e.Brigade)
                    .WithMany()
                    .HasForeignKey(e => e.BrigadeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<LogisticsUnit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Brigade)
                    .WithMany()
                    .HasForeignKey(e => e.BrigadeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CombatEngineeringCompany>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BridgeBuildingCapacity).HasMaxLength(100);
                entity.Property(e => e.FortificationBuildingCapacity).HasMaxLength(100);
                entity.Property(e => e.ObstacleClearingCapacity).HasMaxLength(100);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Brigade)
                    .WithMany()
                    .HasForeignKey(e => e.BrigadeId)
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

            modelBuilder.Entity<TokenAreaCoverage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Geometry).IsRequired();
                entity.Property(e => e.CoverageType).HasMaxLength(50);
                entity.Property(e => e.ShapeType).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Token)
                    .WithMany(t => t.AreaCoverages)
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Intelligence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.Property(e => e.Priority).HasMaxLength(20);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Token)
                    .WithMany()
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Recon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReconType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Confidence).HasMaxLength(20);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Token)
                    .WithMany()
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Suspected Token entities (Fog of War)
            modelBuilder.Entity<SuspectedToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PlacerSide).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.SuspectedType).HasMaxLength(100);
                entity.Property(e => e.MarkerStyle).HasMaxLength(50);
                entity.Property(e => e.VisibleTo).HasMaxLength(200);

            });

            modelBuilder.Entity<ISRMission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AssetType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.RequestedBy).HasMaxLength(100);

                entity.HasOne(e => e.SuspectedToken)
                    .WithMany(st => st.ISRMissions)
                    .HasForeignKey(e => e.SuspectedTokenId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            // Configure Attack Order entities
            modelBuilder.Entity<AttackOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AttackerTokenId).IsRequired();
                entity.Property(e => e.TargetTokenId).IsRequired();
                entity.Property(e => e.AxisId).HasMaxLength(50);
                entity.Property(e => e.ArtilleryAttached).HasMaxLength(1000);
                entity.Property(e => e.Posture).HasMaxLength(20);
                entity.Property(e => e.ExecutionMode).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.PayloadJson).HasMaxLength(2000);

                // Configure relationships
                entity.HasOne(e => e.AttackerToken)
                    .WithMany()
                    .HasForeignKey(e => e.AttackerTokenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TargetToken)
                    .WithMany()
                    .HasForeignKey(e => e.TargetTokenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for performance
                entity.HasIndex(e => e.ExpectedStartTurn);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.AttackerTokenId);
                entity.HasIndex(e => e.TargetTokenId);
            });

            // Configure Defense Element entities
            modelBuilder.Entity<DefenseElement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ElementId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Coordinates).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Visibility).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Metadata).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Configure relationships
                entity.HasOne(e => e.Token)
                    .WithMany()
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.GameSession)
                    .WithMany()
                    .HasForeignKey(e => e.GameSessionId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for performance
                entity.HasIndex(e => e.ElementId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.TokenId);
                entity.HasIndex(e => e.GameSessionId);
                entity.HasIndex(e => e.Status);
            });

            // Configure Map Data entities
            modelBuilder.Entity<MapRegion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Geometry).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Properties).HasColumnType("nvarchar(max)");
                entity.Property(e => e.RegionType).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            modelBuilder.Entity<MapSector>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Geometry).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.LandType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Properties).HasColumnType("nvarchar(max)");
                entity.Property(e => e.SectorType).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            modelBuilder.Entity<MapLabel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LabelType).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Icon).HasMaxLength(50);
                entity.Property(e => e.FontWeight).HasMaxLength(50);
                entity.Property(e => e.Properties).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            modelBuilder.Entity<MapConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConfigurationType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Value).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Properties).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Description).HasMaxLength(1000);
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

            // Configure WarGameSimulation entities (UnitDeployment, MovementOrder, Battle, etc.)
            // Note: Full UnitDeployment configuration is at end of OnModelCreating
            
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
                entity.Property(e => e.Position).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ProtectionType).HasMaxLength(50);
                entity.Property(e => e.Equipment).HasColumnType("nvarchar(max)");

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

            modelBuilder.Entity<MapDocument>(entity =>
            {
                entity.Property(p => p.RegionsJson).HasColumnType("nvarchar(max)");
                entity.Property(p => p.ObstaclesJson).HasColumnType("nvarchar(max)");
                entity.Property(p => p.SafeJson).HasColumnType("nvarchar(max)");
            });

            // Phase 01 Model Configurations
            modelBuilder.Entity<TerrainType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TerrainCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MovementCostRoad).HasPrecision(18, 2);
                entity.Property(e => e.MovementCostCrossCountry).HasPrecision(18, 2);
                entity.Property(e => e.CombatModifier).HasPrecision(18, 2);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<RoutesDraft>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RouteName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.WaypointsJson).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.TotalDistanceKm).HasPrecision(18, 2);
                entity.Property(e => e.SupplyImpact).HasPrecision(18, 2);

                entity.HasOne(e => e.UnitDeployment)
                    .WithMany()
                    .HasForeignKey(e => e.UnitId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure UnitDeployment entity (complete configuration)
            modelBuilder.Entity<UnitDeployment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Required properties
                entity.Property(e => e.UnitType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ForceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Position).IsRequired().HasColumnType("nvarchar(max)");
                
                // Optional properties
                entity.Property(e => e.Formation).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.CurrentTerrain).HasMaxLength(32);
                entity.Property(e => e.SupplyState).HasMaxLength(8);
                
                // Decimal precision properties
                entity.Property(e => e.CombatPowerIndex).HasPrecision(18, 2);
                entity.Property(e => e.EffectiveCombatPower_RO).HasPrecision(18, 2);
                entity.Property(e => e.StrengthPercentage).HasPrecision(18, 2);
                entity.Property(e => e.TerrainModifier).HasPrecision(18, 2);
                entity.Property(e => e.SupplyModifier).HasPrecision(18, 2);
                entity.Property(e => e.Morale).HasPrecision(18, 2);
                entity.Property(e => e.Fatigue).HasPrecision(18, 2);

                // Foreign key relationship
                entity.HasOne(e => e.Scenario)
                    .WithMany(e => e.UnitDeployments)
                    .HasForeignKey(e => e.ScenarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changeSet = this.ChangeTracker.Entries<BaseEntity>();
            if (Session != null)
            {
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
                            entry.Entity.TeamId = userDetails.TeamId;
                        }
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
