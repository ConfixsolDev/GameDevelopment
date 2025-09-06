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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.TrainingConsistency).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Team)
                    .WithMany(e => e.Tokens)
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TokenGroup)
                    .WithMany(e => e.Tokens)
                    .HasForeignKey(e => e.TokenGroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.TeamId);
                entity.Property(e=>e.Id).ValueGeneratedNever();

            });

            // Configure TokenSignature entity
            modelBuilder.Entity<TokenSignature>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenHash).HasMaxLength(500);
                entity.Property(e => e.OriginalTouches).HasColumnType("nvarchar(max)");

                entity.HasOne(e => e.Token)
                    .WithOne(e => e.Signature)
                    .HasForeignKey<TokenSignature>(e => e.TokenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TokenId);
                entity.HasIndex(e => e.Timestamp);
            });

            // Configure StabilityInfo entity
            modelBuilder.Entity<StabilityInfo>(entity =>
            {
                entity.HasKey(e => e.TokenSignatureId);
                entity.HasOne(e => e.TokenSignature)
                    .WithOne(e => e.Stability)
                    .HasForeignKey<StabilityInfo>(e => e.TokenSignatureId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TouchGeometry entity
            modelBuilder.Entity<TouchGeometry>(entity =>
            {
                entity.HasKey(e => e.TokenSignatureId);
                entity.Property(e => e.RadiusValues).HasColumnType("nvarchar(max)");
                entity.Property(e => e.RotationValues).HasColumnType("nvarchar(max)");
                entity.Property(e => e.AvgRadius).HasColumnType("decimal(10,4)");
                entity.Property(e => e.AvgRotation).HasColumnType("decimal(10,4)");
                entity.Property(e => e.RadiusVariance).HasColumnType("decimal(10,4)");

                entity.HasOne(e => e.TokenSignature)
                    .WithOne(e => e.TouchProperties)
                    .HasForeignKey<TouchGeometry>(e => e.TokenSignatureId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TouchPattern entity
            modelBuilder.Entity<TouchPattern>(entity =>
            {
                entity.HasKey(e => e.TokenSignatureId);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Distances).HasColumnType("nvarchar(max)");
                entity.Property(e => e.DistancePairs).HasColumnType("nvarchar(max)");
                entity.Property(e => e.GeometricCenter).HasColumnType("nvarchar(max)");
                entity.Property(e => e.DistanceSignature).HasMaxLength(500);
                entity.Property(e => e.AvgDistance).HasColumnType("decimal(10,4)");
                entity.Property(e => e.MinDistance).HasColumnType("decimal(10,4)");
                entity.Property(e => e.MaxDistance).HasColumnType("decimal(10,4)");
                entity.Property(e => e.DistanceRange).HasColumnType("decimal(10,4)");
                entity.Property(e => e.DistanceVariance).HasColumnType("decimal(10,4)");
                entity.Property(e => e.AngleSpread).HasColumnType("decimal(10,4)");

                entity.HasOne(e => e.TokenSignature)
                    .WithOne(e => e.TouchPattern)
                    .HasForeignKey<TouchPattern>(e => e.TokenSignatureId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MultiTouchGeometry entity
            modelBuilder.Entity<MultiTouchGeometry>(entity =>
            {
                entity.HasKey(e => e.TokenSignatureId);
                entity.Property(e => e.AspectRatio).HasColumnType("decimal(10,4)");
                entity.Property(e => e.BoundingBoxWidth).HasColumnType("decimal(10,4)");
                entity.Property(e => e.BoundingBoxHeight).HasColumnType("decimal(10,4)");
                entity.Property(e => e.BoundingBoxArea).HasColumnType("decimal(10,4)");
                entity.Property(e => e.CenterX).HasColumnType("decimal(10,4)");
                entity.Property(e => e.CenterY).HasColumnType("decimal(10,4)");
                entity.Property(e => e.Spread).HasColumnType("decimal(10,4)");
                entity.Property(e => e.Density).HasColumnType("decimal(10,4)");

                entity.HasOne(e => e.TokenSignature)
                    .WithOne(e => e.MultiTouchGeometry)
                    .HasForeignKey<MultiTouchGeometry>(e => e.TokenSignatureId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MapMarker entity
            modelBuilder.Entity<MapMarker>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TokenName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(e => e.Token)
                    .WithMany(e => e.MapMarkers)
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TokenId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
            });


            // Configure Team entity
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TeamCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubTeamCode).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedByUserId).HasMaxLength(50);
                entity.Property(e => e.CreatedByUserName).HasMaxLength(50);

                entity.HasIndex(e => e.TeamCode);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure TokenGroup entity
            modelBuilder.Entity<TokenGroup>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.GroupCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedByUserId).HasMaxLength(50);
                entity.Property(e => e.CreatedByUserName).HasMaxLength(50);

                entity.HasIndex(e => e.GroupCode);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure TeamTokenGroupAssignment entity
            modelBuilder.Entity<TeamTokenGroupAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssignedByUserId).HasMaxLength(50);
                entity.Property(e => e.AssignedByUserName).HasMaxLength(50);

                entity.HasOne(e => e.Team)
                    .WithMany(e => e.TokenGroupAssignments)
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TokenGroup)
                    .WithMany(e => e.TeamAssignments)
                    .HasForeignKey(e => e.TokenGroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.TokenGroupId);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure GameSession entity
            modelBuilder.Entity<GameSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.SessionCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

                entity.HasIndex(e => e.SessionCode);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartTime);
            });

            // Configure TokenBinding entity
            modelBuilder.Entity<TokenBinding>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityName).HasMaxLength(100);
                entity.Property(e => e.EntityCode).HasMaxLength(50);
                entity.Property(e => e.EntityDescription).HasMaxLength(500);
                entity.Property(e => e.BindingType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.BoundByUserId).HasMaxLength(50);
                entity.Property(e => e.BoundByUserName).HasMaxLength(50);
                entity.Property(e => e.UnboundByUserId).HasMaxLength(50);
                entity.Property(e => e.UnboundByUserName).HasMaxLength(50);

                entity.HasOne(e => e.GameSession)
                    .WithMany(e => e.TokenBindings)
                    .HasForeignKey(e => e.GameSessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Token)
                    .WithMany()
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TokenGroup)
                    .WithMany()
                    .HasForeignKey(e => e.TokenGroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.GameSessionId);
                entity.HasIndex(e => e.TokenId);
                entity.HasIndex(e => e.TokenGroupId);
                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.BoundAt);
            });

            // Configure FreeToken entity
            modelBuilder.Entity<FreeToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.System).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedByUserName).HasMaxLength(50);
                entity.Property(e => e.UnboundByUserId).HasMaxLength(50);
                entity.Property(e => e.UnboundByUserName).HasMaxLength(50);
                entity.Property(e => e.Distances).HasMaxLength(1000);
                entity.Property(e => e.Angles).HasMaxLength(1000);
                entity.Property(e => e.Center).HasMaxLength(100);
                entity.Property(e => e.ComplexSignature).HasMaxLength(2000);

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.System);
                entity.HasIndex(e => e.LastUsed);
            });

            // Configure MapMarker entity (keep for map functionality)
            modelBuilder.Entity<MapMarker>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TokenName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(e => e.Token)
                    .WithMany(e => e.MapMarkers)
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TokenId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
            });

        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var changeSet = this.ChangeTracker.Entries();
                ApplicationUserVM userDetails = null;
                
                // Safely get user details from session if available
                if (Session != null)
                {
                    try
                    {
                        userDetails = Session.GetObject<ApplicationUserVM>(AppConstants.UserSessionKey);
                    }
                    catch
                    {
                        // Session might not be available during database operations
                        userDetails = null;
                    }
                }

                if (changeSet != null)
                {
                    foreach (var entry in changeSet.Where(c => c.State != EntityState.Unchanged))
                    {
                        // Only process entities that inherit from BaseEntity
                        if (entry.Entity is BaseEntity baseEntity)
                        {
                            var now = DateTime.Now;
                            if (entry.State == EntityState.Added)
                            {
                                baseEntity.Id = Guid.NewGuid();
                                baseEntity.CreatedBy = userDetails?.UserCode ?? "System";
                                baseEntity.CreatedDate = now;
                                baseEntity.CreatedAt = now;
                                baseEntity.UpdatedBy = userDetails?.UserCode ?? "System";
                                baseEntity.UpdatedDate = now;
                            }
                            else if (entry.State == EntityState.Modified)
                            {
                                var originalValues = entry.GetDatabaseValues();
                                if (originalValues != null)
                                {
                                    baseEntity.UpdatedBy = userDetails?.UserCode ?? "System";
                                    baseEntity.UpdatedDate = now;

                                    // Preserve original creation values
                                    if (originalValues["CreatedDate"] != null)
                                        baseEntity.CreatedDate = (DateTime)originalValues["CreatedDate"];
                                    if (originalValues["CreatedBy"] != null)
                                        baseEntity.CreatedBy = (string)originalValues["CreatedBy"];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log error if needed, but don't fail the save operation
            }
            
            return await base.SaveChangesAsync(cancellationToken);
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
        public DbSet<TokenBinding> TokenBindings { get; set; }
        public DbSet<FreeToken> FreeTokens { get; set; }

        private T GenerateOriginalEntity<T>(PropertyValues values) where T : new()
        {
            T entity = new T();
            Type type = typeof(T);
            var baseProperties = type.GetProperties();
            foreach (var property in baseProperties)
            {
                if (values[property.Name] != null)
                {
                    property.SetValue(entity, values[property.Name]);
                }
            }
            return entity;
        }
    }
}
