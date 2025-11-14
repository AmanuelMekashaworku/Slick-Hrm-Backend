using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using slick.Application.DependencyInjection;
using slick.Application.Security;
using slick.Application.Services.Implementations;
using slick.Application.Services.Interfaces;
using slick.infrastructure.DependencyInjection;
using slick.Infrastructure.Services;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Setup Serilog logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("log/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

Log.Logger.Information("Application is building....");

// Add core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register infrastructure and application services
builder.Services.AddInfrastructureService(builder.Configuration);
builder.Services.AddApplicationService();

// Register custom permission-based authorization
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();
// Setup backup directory
var backupDirectory = builder.Configuration.GetValue<string>("BackupSettings:BackupDirectory") ?? "Backups";
var absoluteBackupDirectory = Path.Combine(AppContext.BaseDirectory, backupDirectory);
Directory.CreateDirectory(absoluteBackupDirectory);
builder.Services.AddSingleton(new BackupSettings { BackupDirectory = absoluteBackupDirectory });

// Setup file storage directory
var storagePath = builder.Configuration.GetValue<string>("FileStorageSettings:StoragePath") ?? "AppData";
var absoluteStoragePath = Path.Combine(AppContext.BaseDirectory, storagePath);
Directory.CreateDirectory(absoluteStoragePath);
builder.Services.AddSingleton(new FileStorageSettings { StoragePath = absoluteStoragePath });

// Configure settings from config
builder.Services.Configure<BackupSettings>(builder.Configuration.GetSection("BackupSettings"));
builder.Services.Configure<SMTPSettings>(builder.Configuration.GetSection("SMTPSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration);

// Email service
builder.Services.AddSingleton<IEmailService, EmailService>();

// Register DatabaseBackupService with injected backup directory
builder.Services.AddScoped<IDatabaseBackupService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var backupSettings = sp.GetRequiredService<BackupSettings>();
    return new DatabaseBackupService(config, backupSettings.BackupDirectory);
});

// Register FileStorageService with injected storage path
builder.Services.AddScoped<IFileStorageService>(sp =>
{
    var fileStorageSettings = sp.GetRequiredService<FileStorageSettings>();
    return new FileStorageService(fileStorageSettings.StoragePath);
});

// Configure Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

// JWT Authentication setup with cookie token extraction
var secretKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("The JWT Secret Key is not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CustomBearer";
    options.DefaultChallengeScheme = "CustomBearer";
})
.AddJwtBearer("CustomBearer", options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// Configure CORS with specific origins
builder.Services.AddCors(corsBuilder =>
{
    corsBuilder.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            //.SetIsOriginAllowed(_ => true) // Allow all origins
            .AllowCredentials();
    });
});
// Add services
builder.Services.AddSignalR();
builder.Services.AddScoped<IChatService, ChatService>();
try
{
    var app = builder.Build();

    // Middleware pipeline
    app.UseCors();

    app.UseSerilogRequestLogging();

    // Serve static files from the storage directory
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(absoluteStoragePath),
        RequestPath = "/Files",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600");
        }
    });

    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseInfrastructureService();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // Configure Hangfire Dashboard with basic auth from configuration
    var dashboardOptions = new DashboardOptions
    {
        Authorization = new[]
        {
            new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
            {
                RequireSsl = true,
                SslRedirect = true,
                LoginCaseSensitive = true,
                Users = new[]
                {
                    new BasicAuthAuthorizationUser
                    {
                        Login = builder.Configuration["Hangfire:Username"] ?? "admin",
                        PasswordClear = builder.Configuration["Hangfire:Password"] ?? "admin@123@321"
                    }
                }
            })
        }
    };
    app.UseHangfireDashboard("/hangfire", dashboardOptions);

    app.MapControllers();
    // 🔹 Dynamically register permission policies
    // 🔹 Dynamically register permission policies
    using (var scope = app.Services.CreateScope())
    {
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var authorizationOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        // Get all permissions directly from the service
        var permissions = await permissionService.GetAllPermissionsAsync();

        if (permissions != null && permissions.Any())
        {
            foreach (var permission in permissions
                .Where(p => !string.IsNullOrWhiteSpace(p.PermissionTitle))
                .Select(p => p.PermissionTitle!.Trim())
                .Distinct())
            {
                if (authorizationOptions.GetPolicy(permission) == null)
                {
                    authorizationOptions.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permission)));

                    Log.Information($"✅ Registered dynamic permission policy: {permission}");
                }
            }

            Log.Information($"✅ Total {permissions.Count} permission policies registered dynamically.");
        }
        else
        {
            Log.Warning("⚠️ No permissions found to register as policies.");
        }
    }


    Log.Logger.Information("Application is running");

    using (var scope = app.Services.CreateScope())
    {
        var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();

        var options = new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.Utc,
        };

        RecurringJob.AddOrUpdate<IDatabaseBackupService>(
            recurringJobId: "daily-database-backup",
            methodCall: service => service.BackupAsync(),
            cronExpression: Cron.Daily(11, 0),
            options: options
        );

        Log.Information("Recurring database backup job scheduled.");
    }
    app.Run();
}
catch (Exception ex)
{
    Log.Logger.Error(ex, "Application failed to start....");
}
finally
{
    Log.CloseAndFlush();
}