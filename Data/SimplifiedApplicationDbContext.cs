using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Constants;
using TechWebSol.Extensions;
using TechWebSol.Models;
using TechWebSol.ViewModels;

namespace TechWebSol.Data
{
    /// <summary>
    /// Simplified database context for the token system
    /// Only includes essential tables for geometric pattern matching
    /// </summary>
    public class SimplifiedApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private ISession Session => httpContextAccessor?.HttpContext?.Session;
        
        public SimplifiedApplicationDbContext(DbContextOptions<SimplifiedApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
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

            // Configure simplified Token entity
            modelBuilder.Entity<SimplifiedToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.UsageCount).HasDefaultValue(0);

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            // Configure simplified TokenSignature entity
            modelBuilder.Entity<SimplifiedTokenSignature>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Distances).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Angles).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Center).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Token)
                    .WithOne(e => e.Signature)
                    .HasForeignKey<SimplifiedTokenSignature>(e => e.TokenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TokenId);
                entity.HasIndex(e => e.TouchCount);
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
        
        // DbSets for simplified system
        public DbSet<AppRoles> AppRoles { get; set; }
        public DbSet<SimplifiedToken> Tokens { get; set; }
        public DbSet<SimplifiedTokenSignature> TokenSignatures { get; set; }
        public DbSet<MapMarker> MapMarkers { get; set; }
    }
}
