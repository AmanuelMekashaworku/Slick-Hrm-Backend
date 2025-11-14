using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using slick.Domain.Entities;
using slick.Domain.Entities.Identity;
using slick.Domain.Models;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace slick.infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, ApplicationRole, string,
        IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet properties
        public DbSet<ActionTask> ActionTasks { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<BusinessGroup> BusinessGroups { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<VerificationToken> VerificationTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           
            // Configure Identity tables
            builder.Entity<AppUser>(b =>
            {
                b.ToTable("Users");
                b.HasMany(u => u.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            builder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("Roles");
                b.HasMany(r => r.UserRoles)
                    .WithOne(ur => ur.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
                b.HasMany(r => r.RolePermissions)
                    .WithOne(rp => rp.Role)
                    .HasForeignKey(rp => rp.RoleId);
            });

            builder.Entity<UserRole>(b =>
            {
                b.ToTable("UserRoles");
                b.HasKey(ur => new { ur.UserId, ur.RoleId }); // Composite key

                b.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);

                b.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);
            });

            builder.Entity<RolePermission>(b =>
            {
                b.HasKey(rp => rp.Id);
                b.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId);
                b.HasOne(rp => rp.Permission)
                    .WithMany()
                    .HasForeignKey(rp => rp.PermissionId);
            });
            // Index on CreatedDate for sorting recent messages
            builder.Entity<ChatMessage>()
                .HasIndex(cm => cm.CreatedDate)
                .HasDatabaseName("IX_ChatMessages_CreatedDate");

            // Index on UserId for filtering by user
            builder.Entity<ChatMessage>()
                .HasIndex(cm => cm.UserId)
                .HasDatabaseName("IX_ChatMessages_UserId");

            // Composite index on UserId and CreatedDate for user-specific recent messages
            builder.Entity<ChatMessage>()
                .HasIndex(cm => new { cm.UserId, cm.CreatedDate })
                .HasDatabaseName("IX_ChatMessages_UserId_CreatedDate");
            // Optional: Composite index on UserId and CreatedDate (if sorting user-specific messages)
            builder.Entity<ChatMessage>()
                .HasIndex(cm => new { cm.UserId, cm.CreatedDate })
                .HasDatabaseName("IX_ChatMessages_UserId_CreatedDate");
            // Configure other relationships
            ConfigureRelationships(builder);
            ConfigureEntityProperties(builder);
            SeedInitialData(builder);
            ApplySoftDeleteGlobalFilters(builder);
        }

        private void ConfigureRelationships(ModelBuilder builder)
        {
            builder.Entity<BusinessGroup>()
                .HasMany(bg => bg.Companies)
                .WithOne(c => c.BusinessGroup)
                .HasForeignKey(c => c.BusinessGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Company>()
                .HasMany(c => c.Branches)
                .WithOne(b => b.Company)
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Add other relationships as needed
        }

        private void ConfigureEntityProperties(ModelBuilder builder)
        {
          

            // Configure other entity properties as needed
        }

        private void SeedInitialData(ModelBuilder builder)
        {
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = "a5bda8c9-1d4b-4d4e-bc2d-0d7fbe9a5598",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = new DateTime(2025, 8, 11, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Administrator role with full access"
                },
                new ApplicationRole
                {
                    Id = "d6f3db34-197b-4e6b-b572-d8cf0dff3653",
                    Name = "User",
                    NormalizedName = "USER",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = new DateTime(2025, 8, 11, 0, 0, 0, DateTimeKind.Utc),
                    Description = "Standard user role"
                });

            // Add other seed data as needed
        }

        private void ApplySoftDeleteGlobalFilters(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var condition = Expression.Equal(property, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);

                    entityType.SetQueryFilter(lambda);
                }
            }
        }
    }

    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}