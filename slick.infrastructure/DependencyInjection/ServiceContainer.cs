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
using Microsoft.IdentityModel.Tokens;
using slick.Domain.Models;
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
            services.AddScoped<IGeneric<Company>, GenericRepository<Company>>();
            services.AddScoped<IGeneric<BusinessGroup>, GenericRepository<BusinessGroup>>();
            services.AddScoped<IGeneric<Branch>, GenericRepository<Branch>>();
            services.AddScoped<IGeneric<TaskController>, GenericRepository<TaskController>>();
            services.AddScoped<IGeneric<Permission>, GenericRepository<Permission>>();
            services.AddScoped<IGeneric<ChatMessage>, GenericRepository<ChatMessage>>();
            services.AddScoped<IGeneric<UserActivityLog>, GenericRepository<UserActivityLog>>();
            services.AddScoped<IGeneric<ControllerAction>, GenericRepository<ControllerAction>>();
            services.AddScoped<IGeneric<ActionTask>, GenericRepository<ActionTask>>();
            services.AddScoped<IGeneric<ApplicationRole>, GenericRepository<ApplicationRole>>();
            services.AddScoped<IGeneric<RolePermission>, GenericRepository<RolePermission>>();
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
