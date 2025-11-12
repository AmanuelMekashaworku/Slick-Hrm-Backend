using slick.Domain.Entities;
using slick.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SMS.Domain.Models;
using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

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
        public DbSet<CarSales> CarSaless { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<RTGS> RTGSs { get; set; }
        public DbSet<AccountBalance> AccountBalances { get; set; }
        public DbSet<GRN> GRNs { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<PaymentRequestSetlment> PaymentRequestSetlments { get; set; }
        public DbSet<PaymentCollection> PaymentCollections { get; set; }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<BusinessGroup> BusinessGroups { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TP_Carrier> TP_Carriers { get; set; }
        public DbSet<Campus> Campuses { get; set; }
        public DbSet<PerformaItemDetail> PerformaItemDetails { get; set; }
        public DbSet<CancilationPolicy> CancilationPolicys { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarBodyType> CarBodyTypes { get; set; }
        public DbSet<CarBrand> CarBrands { get; set; }
        public DbSet<CarColor> CarColors { get; set; }
        public DbSet<CarDriveType> CarDriveTypes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyAccount> CompanyAccounts { get; set; }
        public DbSet<ControllerAction> ControllersActions { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DispatchVehicle> DispatchVehicles { get; set; }
        public DbSet<FuelType> FuelTypes { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<SpecialInstruction> SpecialInstructions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierType> SupplierTypes { get; set; }
        public DbSet<TaskController> TaskControllers { get; set; }
        public DbSet<TermandCondition> TermandConditions { get; set; }
        public DbSet<TP_CancilationPolicy> TP_CancilationPolicys { get; set; }
        public DbSet<TP_PaymentMethod> TP_PaymentMethods { get; set; }
        public DbSet<TP_TermandCondition> TP_TermandConditions { get; set; }
        public DbSet<TP_SpecialInstruction> TP_SpecialInstructions { get; set; }
        public DbSet<PerformaItem> PerformaItems { get; set; }
        public DbSet<CP_CancilationPolicy> CP_CancilationPolicys { get; set; }
        public DbSet<CP_TermandCondition> CP_TermandConditions { get; set; }
        public DbSet<CP_SpecialInstruction> CP_SpecialInstructions { get; set; }
        public DbSet<CP_PaymentMethod> CP_PaymentMethods { get; set; }
        public DbSet<CP_CompanyAccount> CP_CompanyAccounts { get; set; }
        public DbSet<TransportPerforma> TransportPerformas { get; set; }
        public DbSet<CarPerforma> CarPerforma { get; set; }
        public DbSet<VerificationToken> VerificationTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public override DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RTGS>()
               .HasOne(r => r.FromCompanyAccount)
               .WithMany()
               .HasForeignKey(r => r.FromCompanyAccountId)
               .OnDelete(DeleteBehavior.Restrict); // Prevent cascade
            builder.Entity<RTGS>()
                .HasOne(r => r.ToCompanyAccount)
                .WithMany()
                .HasForeignKey(r => r.ToCompanyAccountId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade
            builder.Entity<ApplicationRole>()
                .HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .HasPrincipalKey(r => r.Id)
                .OnDelete(DeleteBehavior.Cascade);
          
            builder.Entity<CarPerforma>()
                .HasMany(cp => cp.PerformaItemIds)
                .WithOne(pi => pi.CarPerforma)
                .HasForeignKey(pi => pi.CarPerformId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarPerforma>()
                .HasMany(cp => cp.CancilationPolicyIds)
                .WithOne(c => c.CarPerforma)
                .HasForeignKey(c => c.CarPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarPerforma>()
                .HasMany(cp => cp.PaymentMethodIds)
                .WithOne(pm => pm.CarPerforma)
                .HasForeignKey(pm => pm.CarPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarPerforma>()
                .HasMany(cp => cp.CompanyAccountsIds)
                .WithOne(ca => ca.CarPerforma)
                .HasForeignKey(ca => ca.CarPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarPerforma>()
                .HasMany(cp => cp.SpecialInstructionIds)
                .WithOne(si => si.CarPerforma)
                .HasForeignKey(si => si.CarPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarPerforma>()
                .HasMany(cp => cp.TermandConditionIds)
                .WithOne(tc => tc.CarPerforma)
                .HasForeignKey(tc => tc.CarPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarPerforma>()
                   .HasOne(cp => cp.ToCompany)
                   .WithMany()
                   .HasForeignKey(cp => cp.ToCompanyId)
                   .OnDelete(DeleteBehavior.NoAction); // Use NoAction

            builder.Entity<CarPerforma>()
                    .HasOne(cp => cp.OnBehalfCompany)
                    .WithMany()
                    .HasForeignKey(cp => cp.OnBehalfCompanyId)
                    .OnDelete(DeleteBehavior.NoAction); // Use NoAction
                                                        //trnasportstart
            builder.Entity<TransportPerforma>()
              .HasMany(cp => cp.PerformaItemIds)
              .WithOne(pi => pi.TransportPerforma)
              .HasForeignKey(pi => pi.TransportPerformaId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TransportPerforma>()
                .HasMany(cp => cp.CancilationPolicyIds)
                .WithOne(c => c.TransportPerforma)
                .HasForeignKey(c => c.TransportPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TransportPerforma>()
                .HasMany(cp => cp.PaymentMethodIds)
                .WithOne(pm => pm.TransportPerforma)
                .HasForeignKey(pm => pm.TransportPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TransportPerforma>()
                .HasMany(cp => cp.CompanyAccountsIds)
                .WithOne(ca => ca.TransportPerforma)
                .HasForeignKey(ca => ca.TransportPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TransportPerforma>()
                .HasMany(cp => cp.SpecialInstructionIds)
                .WithOne(si => si.TransportPerforma)
                .HasForeignKey(si => si.TransportPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TransportPerforma>()
                .HasMany(cp => cp.TermandConditionIds)
                .WithOne(tc => tc.TransportPerforma)
                .HasForeignKey(tc => tc.TransportPerformaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TransportPerforma>()
                   .HasOne(cp => cp.ToCompany)
                   .WithMany()
                   .HasForeignKey(cp => cp.ToCompanyId)
                   .OnDelete(DeleteBehavior.NoAction); // Use NoAction

            builder.Entity<TransportPerforma>()
                    .HasOne(cp => cp.OnBehalfOfCompany)
                    .WithMany()
                    .HasForeignKey(cp => cp.OnBehalfcompanyId)
                    .OnDelete(DeleteBehavior.NoAction); // Use NoAction
            //endtrnasport
            builder.Entity<GRN>()
                    .HasMany(g => g.GRNItems)
                    .WithOne(i => i.GRN)
                    .HasForeignKey(i => i.GrnId)
                    .IsRequired() // This makes the foreign key required (non-nullable)
                    .OnDelete(DeleteBehavior.Cascade);


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
            builder.Entity<CarBrand>(entity =>
            {
                entity.Property(c => c.BrandName)
                    .HasMaxLength(100);
                entity.Property(c => c.Description)
                    .HasMaxLength(250);
            });

            builder.Entity<CarColor>(entity =>
            {
                entity.Property(c => c.ColorName)
                    .IsRequired();
            });

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