using slick.Application.Mapping;
using slick.Application.Services.Implementations;
using slick.Application.Services.Implementations.Authentication;
using slick.Application.Services.Implmentations;
using slick.Application.Services.Interfaces;
using slick.Application.Services.Interfaces.Authentication;
using slick.Application.Validations;
using slick.Application.Validations.Authentication;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;





namespace slick.Application.DependencyInjection
{
    public static  class ServiceContainer
    {
     public static IServiceCollection AddApplicationService(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingConfig));
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IBusinessGroupService, BusinessGroupService>();
        services.AddScoped<ITaskControllerService, TaskControllerService>();
        services.AddScoped<IControllerActionService, ControllerActionService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ITaskActionService, TaskActionService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserActivityLogService, UserActivityLogService>();
        services.AddScoped<ITaskActionService, TaskActionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();

            return services;
    }
    }
}
