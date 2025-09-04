using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net.Mail;
using System.Net.Sockets;
using TechWebSol.Areas.Mail.Models;
using TechWebSol.Constants;
using TechWebSol.Extensions;
using TechWebSol.Models;
using TechWebSol.Models.DocumentModal;
using TechWebSol.Models.PersonalModals;
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
        
        public DbSet<AppRoles> AppRoles { get; set; }
        
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
