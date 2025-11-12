using EntityFramework.Exceptions.SqlServer;
using slick.Application.Services.Implementations;
using slick.Application.Services.Implmentations;
using slick.Application.Services.Interfaces;
using slick.Application.Services.Interfaces.Logging;
using slick.Domain.Entities;
using slick.Domain.Entities.Identity;
using slick.Domain.Interfaces;
using slick.Domain.Interfaces.Authentication;
using slick.infrastructure.Data;
using slick.infrastructure.MIddleware;
using slick.infrastructure.Repositories;
using slick.infrastructure.Repositories.Authentication;
using slick.infrastructure.Services;
using slick.Infrastructure.Authentication;
using slick.Infrastructure.Repositories.Authentication;
using slick.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using SMS.Domain.Models;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;

namespace slick.infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {

        public static IServiceCollection AddInfrastructureService
            (this IServiceCollection services, IConfiguration config)
        {
            string connectionString = "Default";
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString(connectionString),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure();
            }).UseExceptionProcessor(),
            ServiceLifetime.Scoped);
            services.AddScoped<IGeneric<Campus>, GenericRepository<Campus>>();
            services.AddScoped<IGeneric<CarSales>, GenericRepository<CarSales>>();
            services.AddScoped<IGeneric<Company>, GenericRepository<Company>>();
            services.AddScoped<IGeneric<Loan>, GenericRepository<Loan>>();
            services.AddScoped<IGeneric<RTGS>, GenericRepository<RTGS>>();
            services.AddScoped<IGeneric<BusinessGroup>, GenericRepository<BusinessGroup>>();
            services.AddScoped<IGeneric<PaymentRequestSetlment>, GenericRepository<PaymentRequestSetlment>>();
            services.AddScoped<IGeneric<Branch>, GenericRepository<Branch>>();
            services.AddScoped<IGeneric<Supplier>, GenericRepository<Supplier>>();
            services.AddScoped<IGeneric<SupplierType>, GenericRepository<SupplierType>>();
            services.AddScoped<IGeneric<CompanyAccount>, GenericRepository<CompanyAccount>>();
            services.AddScoped<IGeneric<CarColor>, GenericRepository<CarColor>>();
            services.AddScoped<IGeneric<PaymentCollection>, GenericRepository<PaymentCollection>>();
            services.AddScoped<IGeneric<GRN>, GenericRepository<GRN>>();
            services.AddScoped<IGeneric<CarBrand>, GenericRepository<CarBrand>>();
            services.AddScoped<IGeneric<CarBodyType>, GenericRepository<CarBodyType>>();
            services.AddScoped<IGeneric<CarDriveType>, GenericRepository<CarDriveType>>();
            services.AddScoped<IGeneric<Notification>, GenericRepository<Notification>>();
            services.AddScoped<IGeneric<Customer>, GenericRepository<Customer>>();
            services.AddScoped<IGeneric<AccountBalance>, GenericRepository<AccountBalance>>();
            services.AddScoped<IGeneric<Car>, GenericRepository<Car>>();
            services.AddScoped<IGeneric<FuelType>, GenericRepository<FuelType>>();
            services.AddScoped<IGeneric<TaskController>, GenericRepository<TaskController>>();
            services.AddScoped<IGeneric<Permission>, GenericRepository<Permission>>();
            services.AddScoped<IGeneric<ControllerAction>, GenericRepository<ControllerAction>>();
            services.AddScoped<IGeneric<PaymentRequest>, GenericRepository<PaymentRequest>>();
            services.AddScoped<IGeneric<ActionTask>, GenericRepository<ActionTask>>();
            services.AddScoped<IGeneric<ApplicationRole>, GenericRepository<ApplicationRole>>();
            services.AddScoped<IGeneric<RolePermission>, GenericRepository<RolePermission>>();
            services.AddScoped<IGeneric<CarSales>, GenericRepository<CarSales>>();
            services.AddScoped<IGeneric<GRNItem>, GenericRepository<GRNItem>>();
            services.AddScoped<IGeneric<CancilationPolicy>, GenericRepository<CancilationPolicy>>();
            services.AddScoped<IGeneric<PaymentMethod>, GenericRepository<PaymentMethod>>();
            services.AddScoped<IGeneric<TermandCondition>, GenericRepository<TermandCondition>>();
            services.AddScoped<IGeneric<SpecialInstruction>, GenericRepository<SpecialInstruction>>();
            services.AddScoped<IGeneric<CarPerforma>, GenericRepository<CarPerforma>>();
            services.AddScoped<IGeneric<PerformaItem>, GenericRepository<PerformaItem>>();
            services.AddScoped<IGeneric<CP_CancilationPolicy>, GenericRepository<CP_CancilationPolicy>>();
            services.AddScoped<IGeneric<CP_PaymentMethod>, GenericRepository<CP_PaymentMethod>>();
            services.AddScoped<IGeneric<CP_SpecialInstruction>, GenericRepository<CP_SpecialInstruction>>();
            services.AddScoped<IGeneric<CP_TermandCondition>, GenericRepository<CP_TermandCondition>>();
            services.AddScoped<IGeneric<TP_CancilationPolicy>, GenericRepository<TP_CancilationPolicy>>();
            services.AddScoped<IGeneric<TP_PaymentMethod>, GenericRepository<TP_PaymentMethod>>();
            services.AddScoped<IGeneric<TP_SpecialInstruction>, GenericRepository<TP_SpecialInstruction>>();
            services.AddScoped<IGeneric<TP_TermandCondition>, GenericRepository<TP_TermandCondition>>();
            services.AddScoped<IGeneric<TP_Carrier>, GenericRepository<TP_Carrier>>();
            services.AddScoped<IGeneric<TransportPerforma>, GenericRepository<TransportPerforma>>();
            services.AddScoped<IGeneric<Carrier>, GenericRepository<Carrier>>();
            services.AddScoped<IGeneric<AppUser>, GenericRepository<AppUser>>();
            services.AddScoped(typeof(IAppLogger<>), typeof(SerilogLoggerAdapter<>));

            services.AddDefaultIdentity<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;
            })
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey= true,
                    ValidIssuer = config["JWT:Issuer"],
                    ValidAudience = config["JWT:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]!))

                };
            }
                );
            services.AddScoped<IUserManagement, UserManagement>();
            services.AddScoped<ITokenManagement, TokenManagement>();
            services.AddScoped<IRoleManagement, RoleManagement>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();
            return services;
        }
        public static IApplicationBuilder UseInfrastructureService(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }

    }
}
