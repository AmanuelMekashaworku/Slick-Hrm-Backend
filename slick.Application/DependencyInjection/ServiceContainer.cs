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
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IRTGSService, RTGSService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<ICarColorService, CarColorService>();
        services.AddScoped<ICarBrandService, CarBrandService>();
        services.AddScoped<ICarBodyTypeService, CarBodyTypeService>();
        services.AddScoped<ICarDriveTypeService, CarDriveTypeService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICarSalesService, CarSalesService>();
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<ICarrierService, CarrierService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IPaymentRequestService, PaymentRequestService>();
        services.AddScoped<IPaymentRequestSettelmentService, PaymentRequestSettelmentService>();
        services.AddScoped<IFuelTypeService, FuelTypeService>();
        services.AddScoped<ICarSalesService, CarSalesService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IGRNService, GRNService>();
        services.AddScoped<ISupplierTypeService, SupplierTypeService>();
        services.AddScoped<ICompanyAccountService, CompanyAccountService>();
        services.AddScoped<IBusinessGroupService, BusinessGroupService>();
        services.AddScoped<ITaskControllerService, TaskControllerService>();
        services.AddScoped<IControllerActionService, ControllerActionService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ITaskActionService, TaskActionService>();
        services.AddScoped<ITermandConditionService, TermandConditionService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICancilationPolicyService, CancilationPolicyService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPaymentCollectionService, PaymentCollectionService>();
        services.AddScoped<ISpecialInstructionService, SpecialInstructionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<ICarPerformaService, CarPerformaService>();
        services.AddScoped<ITransportPerformaService, TransportPerformaService>();

            return services;
    }
    }
}
