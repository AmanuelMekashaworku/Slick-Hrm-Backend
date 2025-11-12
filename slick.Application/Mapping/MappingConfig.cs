    
using AutoMapper;
using slick.Application.DTOs.AccountBalance;
using slick.Application.DTOs.Branch;
using slick.Application.DTOs.BusinessGroup;
using slick.Application.DTOs.Campus;
using slick.Application.DTOs.CancilationPolicy;
using slick.Application.DTOs.Car;
using slick.Application.DTOs.CarBodyType;
using slick.Application.DTOs.CarBrand;
using slick.Application.DTOs.CarColor;
using slick.Application.DTOs.CarDriveType;
using slick.Application.DTOs.CarPerforma;
using slick.Application.DTOs.Carrier;
using slick.Application.DTOs.CarSales;
using slick.Application.DTOs.Company;
using slick.Application.DTOs.CompanyAccount;
using slick.Application.DTOs.ControllerAction;
using slick.Application.DTOs.Customer;
using slick.Application.DTOs.FuelType;
using slick.Application.DTOs.GRN;
using slick.Application.DTOs.Identity;
using slick.Application.DTOs.Loan;
using slick.Application.DTOs.Notification;
using slick.Application.DTOs.PaymentCollection;
using slick.Application.DTOs.PaymentMethod;
using slick.Application.DTOs.PaymentRequest;
using slick.Application.DTOs.PaymentRequestSettelment;
using slick.Application.DTOs.Permission;
using slick.Application.DTOs.Role;
using slick.Application.DTOs.RolePermission;
using slick.Application.DTOs.RTGS;
using slick.Application.DTOs.SpecialInstruction;
using slick.Application.DTOs.Supplier;
using slick.Application.DTOs.SupplierType;
using slick.Application.DTOs.TaskAction;
using slick.Application.DTOs.TaskController;
using slick.Application.DTOs.TermandCondition;
using slick.Application.DTOs.TransportPerforma;
using slick.Domain.Entities;
using slick.Domain.Entities.Identity;
using SMS.Domain.Models;

namespace slick.Application.Mapping
{
    internal class MappingConfig : Profile
    {
         public MappingConfig() 
        {

            //Account Balance mappings
            CreateMap<CreateAccountBalanceDto, AccountBalance>();
            CreateMap<UpdateCompanyAccountDto, AccountBalance>();
            CreateMap<AccountBalance, GetAccountBalanceDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.CompanyAccount.AccountName));

            //Campus mappings
            CreateMap<CreateCampusDto, Campus>();
            CreateMap<UpdateCampusDto, Campus>();
            CreateMap<Campus, GetCampusDto>();
            //CarBrand mappings
            CreateMap<CreateCarBrandDto, CarBrand>();
            CreateMap<UpdateCarBrandDto, CarBrand>();
            CreateMap<CarBrand, GetCarBrandDto>();
            //Car mappings
            CreateMap<CreateCarDto, Car>();
            CreateMap<UpdateCarDto, Car>();
            CreateMap<Car, GetCarDto>()
                .ForMember(dest => dest.CarBodyTypeName, opt => opt.MapFrom(src => src.CarBodyType!.CarBodyTypeName))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.CarBrand!.BrandName))
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.CarColor!.ColorName))
                .ForMember(dest => dest.FuelTypeName, opt => opt.MapFrom(src => src.FuelType!.Name))
                .ForMember(dest => dest.DriveTypeName, opt => opt.MapFrom(src => src.CarDriveType!.DriveTypeName));

            //Car  Color mappings
            CreateMap<CreateCarColorDto, CarColor>();
            CreateMap<UpdateCarColorDto, CarColor>();
            CreateMap<CarColor, GetCarColorDto>();
            //Payment Settlement
            CreateMap<CreatePaymentRequestSettelmentDto, PaymentRequestSetlment>();
            CreateMap<UpdatePaymentRequestSettelmentDto, PaymentRequestSetlment>();
            CreateMap<PaymentRequestSetlment, GetPaymentRequestSettelmentDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.RequestName, opt => opt.MapFrom(src => src.PaymentRequest.RequestName));
            //Car  Color mappings
            CreateMap<CreateCarrierDto, Carrier>();
            CreateMap<UpdateCarrierDto, Carrier>();
            CreateMap<Carrier, GetCarrierDto>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.AppUser.FirstName));
            //Customer mappings
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();
            CreateMap<Customer, GetCustomerDto>();
            //payment Method mappings
            CreateMap<CreatePaymentMethodDto, PaymentMethod>();
            CreateMap<UpdatePaymentMethodDto, PaymentMethod>();
            CreateMap<PaymentMethod, GetPaymentMethodDto>();
            //payment Method mappings
            CreateMap<GRNItem, GRNItemDto>();
            CreateMap<GRNItemDto, GRNItem>();

            CreateMap<CreateGRNDto, GRN>()
                .ForMember(dest => dest.SignedDocument, opt => opt.Ignore())
                .ForMember(dest => dest.GRNItems, opt => opt.MapFrom(src => src.GRNItems));

            CreateMap<UpdateGRNDto, GRN>()
                .ForMember(dest => dest.SignedDocument, opt => opt.Ignore())
                .ForMember(dest => dest.GRNItems, opt => opt.MapFrom(src => src.GRNItems));

            CreateMap<GRN, GetGRNDto>()
                .ForMember(dest => dest.SignedDocument, opt => opt.MapFrom(src => src.SignedDocument))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch!.BranchName))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company!.Name))
                .ForMember(dest => dest.TransportInvoiceNo, opt => opt.MapFrom(src => src.TransportPerforma!.InvoiceNo))
                .ForMember(dest => dest.GRNItems, opt => opt.MapFrom(src => src.GRNItems));



            //Customer mappings
            CreateMap<SupplierType, GetSupplierTypeNameDto>();
            //branch mappings
            CreateMap<CreateBranchDto, Branch>();
            CreateMap<UpdateBranchDto, Branch>();
            CreateMap<Branch, GetBranchDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name));
            //CompanyAccout  mappings
            CreateMap<CreateCompanyAccountDto, CompanyAccount>();
            CreateMap<UpdateCompanyAccountDto, CompanyAccount>();
            CreateMap<CompanyAccount, GetCompanyAccountDto>()
                 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name));

            //Business Group
            CreateMap<CreateBusinessGroupDto, BusinessGroup>();
            CreateMap<UpdateBusinessGroupDto, BusinessGroup>();
            CreateMap<BusinessGroup, GetBusinessGroupDto>();
            //Loan
            CreateMap<CreateLoanDto, Loan>()
                 .ForMember(dest => dest.SleepAttachment, opt => opt.Ignore());
            CreateMap<UpdateLoanDto, Loan>()
                 .ForMember(dest => dest.SleepAttachment, opt => opt.Ignore());
            CreateMap<Loan, GetLoanDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CompanyAccountName, opt => opt.MapFrom(src => src.CompanyAccount!.AccountName));
            //RTGS
            CreateMap<CreateRTGSDto, RTGS>()
                 .ForMember(dest => dest.SleepAttachment, opt => opt.Ignore());
            CreateMap<UpdateRTGSDto, RTGS>()
               .ForMember(dest => dest.SleepAttachment, opt => opt.Ignore());
            CreateMap<RTGS, GetRTGSDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.ToCompanyAccountName, opt => opt.MapFrom(src => src.ToCompanyAccount!.AccountName))
                .ForMember(dest => dest.FromCompanyAccountName, opt => opt.MapFrom(src => src.FromCompanyAccount!.AccountName));

            // Notfication Group
            CreateMap<CreateNotificationDto, Notification>();
            CreateMap<UpdateNotificationDto, Notification>();
            CreateMap<Notification, GetNotificationDto>();

            //Payment Request
            CreateMap<CreatePaymentRequestDto, PaymentRequest>();
            CreateMap<UpdatePaymentRequestDto, PaymentRequest>();
            CreateMap<PaymentRequest, GetPaymentRequestDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.TransportPerforma.InvoiceNo))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.CompanyAccountName, opt => opt.MapFrom(src => src.CompanyAccount.AccountName));

            //car sales 
            CreateMap<CreateCarSalesDto, CarSales>();
            CreateMap<UpdateCarSalesDto, CarSales>();
            CreateMap<CarSales, GetCarSalesDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CarName, opt => opt.MapFrom(src => src.Car!.CarName))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerName : null))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.BranchName : null))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null));
            //Company Group
            CreateMap<CreateCompanyDto, Company>()
                .ForMember(dest => dest.Logo, opt => opt.Ignore());
            CreateMap<UpdateCompanyDto, Company>()
                .ForMember(dest => dest.Logo, opt => opt.Ignore());
            CreateMap<Company, GetCompanyDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.BusinessGroupName, opt => opt.MapFrom(src => src.BusinessGroup.BusinessGroupName))
                .ForMember(dest => dest.BranchNames, opt => opt.MapFrom(src => src.Branches.Select(b => b.BranchName).ToList()))
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.Logo));

            //Company Group
            CreateMap<CreatePaymentCollectionDto, PaymentCollection>()
                .ForMember(dest => dest.SleepAttachment, opt => opt.Ignore());
            CreateMap<UpdatePaymentCollectionDto, PaymentCollection>()
                .ForMember(dest => dest.SleepAttachment, opt => opt.Ignore());
            CreateMap<PaymentCollection, GetPaymentCollectionDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.TransportPerforma!.InvoiceNo))
                .ForMember(dest => dest.CarSaleNumber, opt => opt.MapFrom(src => src.CarSales!.CarSalesNumber))
                .ForMember(dest => dest.CarPerformaInvoiceNumber, opt => opt.MapFrom(src => src.CarPerforma!.InvoiceNumber))
                .ForMember(dest => dest.CompanyAccountName, opt => opt.MapFrom(src => src.CompanyAccount!.AccountName))
                .ForMember(dest => dest.SleepAttachment, opt => opt.MapFrom(src => src.SleepAttachment));
            //Fuel Type Group
            CreateMap<CreateFuelTypeDto, FuelType>();
            CreateMap<UpdateFuelTypeDto, FuelType>();
            CreateMap<FuelType, GetFuelTypeDto>();
            //Car Body Group
            CreateMap<CreateCarBodyTypeDto, CarBodyType>();
            CreateMap<UpdateCarBodyTypeDto, CarBodyType>();
            CreateMap<CarBodyType, GetCarBodyTypeDto>();

            //Car Drive Type Group
            CreateMap<CreateCarDriveTypeDto, CarDriveType>();
            CreateMap<UpdateCarDriveTypeDto, CarDriveType>();
            CreateMap<CarDriveType, GetCarDriveTypeDto>();

            //Term and Condition
            CreateMap<CreateTermandConditionDto, TermandCondition>();
            CreateMap<UpdateTermandConditionDto, TermandCondition>();
            CreateMap<TermandCondition, GetTermandConditionDto>();

            //Special Instruction 
            CreateMap<CreateSpecialInstructionDto, SpecialInstruction>();
            CreateMap<UpdateSpecialInstructionDto, SpecialInstruction>();
            CreateMap<SpecialInstruction, GetSpecialInstructionDto>();

            //CancilationPolicy
            CreateMap<CreateCancilationPolicyDto, CancilationPolicy>();
            CreateMap<UpdateCancilationPolicyDto, CancilationPolicy>();
            CreateMap<CancilationPolicy, GetCancilationPolicyDto>();


            //Role 
            CreateMap<CreateRoleDto, ApplicationRole>();
            CreateMap<UpdateRoleDto, ApplicationRole>();
            CreateMap<ApplicationRole, GetRoleDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions))
                .ForMember(dest => dest.PermissionIds, opt => opt.MapFrom(src => src.RolePermissions!.Select(rp => rp.PermissionId)));

            //transport performa
            CreateMap<PerformaItem, TransportCarItemDto>()
               .ForMember(dest => dest.CarId, opt => opt.MapFrom(src => src.CarId))
               .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Qunatity))
               .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
               .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));
            CreateMap<CreateTransportPerformaDto, TransportPerforma>()
                .ForMember(dest => dest.CancilationPolicyIds, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodIds, opt => opt.Ignore())
                .ForMember(dest => dest.SpecialInstructionIds, opt => opt.Ignore())
                .ForMember(dest => dest.TermandConditionIds, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyAccountsIds, opt => opt.Ignore())
                .ForMember(dest => dest.PerformaItemIds, opt => opt.Ignore());
            CreateMap<UpdateTransportPerformaDto, TransportPerforma>()
                .ForMember(dest => dest.PaymentMethodIds, opt => opt.Ignore())
                .ForMember(dest => dest.CancilationPolicyIds, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyAccountsIds, opt => opt.Ignore())
                .ForMember(dest => dest.SpecialInstructionIds, opt => opt.Ignore())
                .ForMember(dest => dest.TermandConditionIds, opt => opt.Ignore())
                .ForMember(dest => dest.CarriersIds, opt => opt.Ignore())
                .ForMember(dest => dest.PerformaItemIds, opt => opt.Ignore())
                .ForMember(dest => dest.ShipmentDocument, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryOrderDocument, opt => opt.Ignore());
            CreateMap<TransportPerforma, GetTransportPerformaDto>()
            // Collections
                .ForMember(dest => dest.cancellationPolicyIds, opt => opt.MapFrom(src => src.CancilationPolicyIds.Select(rp => rp.CancilationPolicyId)))
                .ForMember(dest => dest.paymentMethodIds, opt => opt.MapFrom(src => src.PaymentMethodIds.Select(rp => rp.PaymentMethodId)))
                .ForMember(dest => dest.specialInstructionIds, opt => opt.MapFrom(src => src.SpecialInstructionIds.Select(rp => rp.SpecialInstructionId)))
                .ForMember(dest => dest.termAndConditionIds, opt => opt.MapFrom(src => src.TermandConditionIds.Select(rp => rp.TermandConditionId)))
                .ForMember(dest => dest.companyAccountIds, opt => opt.MapFrom(src => src.CompanyAccountsIds.Select(rp => rp.CompanyAccountId)))
                .ForMember(dest => dest.carriersIds, opt => opt.MapFrom(src => src.CarriersIds.Select(rp => rp.CarrierId)))
                // IDs
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.ToCompanyId, opt => opt.MapFrom(src => src.ToCompanyId))
                // Names
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerName : null))
                .ForMember(dest => dest.ToCompanyName, opt => opt.MapFrom(src => src.ToCompany != null ? src.ToCompany.Name : null))
                .ForMember(dest => dest.OnBehalfCompanyName, opt => opt.MapFrom(src => src.OnBehalfOfCompany != null ? src.OnBehalfOfCompany.Name : null))
                // CarItems mapping including Details
                .ForMember(dest => dest.TransportCarItems, opt => opt.MapFrom(src => src.PerformaItemIds))
                .ForMember(dest => dest.ShipmentDocument, opt => opt.MapFrom(src => src.ShipmentDocument))
                .ForMember(dest => dest.DeliveryOrderDocument, opt => opt.MapFrom(src => src.DeliveryOrderDocument));




            //Car Performa mappings
            CreateMap<PerformaItem, CarItemDto>()
              .ForMember(dest => dest.CarId, opt => opt.MapFrom(src => src.CarId))
              .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Qunatity))
              .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
              .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));
            CreateMap<CreateCarPerformaDto, CarPerforma>()
                .ForMember(dest => dest.CancilationPolicyIds, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodIds, opt => opt.Ignore())
                .ForMember(dest => dest.SpecialInstructionIds, opt => opt.Ignore())
                .ForMember(dest => dest.TermandConditionIds, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyAccountsIds, opt => opt.Ignore())
                .ForMember(dest => dest.PerformaItemIds, opt => opt.Ignore());

            CreateMap<UpdateCarPerformaDto, CarPerforma>()
                .ForMember(dest => dest.PaymentMethodIds, opt => opt.Ignore())
                .ForMember(dest => dest.CancilationPolicyIds, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyAccountsIds, opt => opt.Ignore())
                .ForMember(dest => dest.SpecialInstructionIds, opt => opt.Ignore())
                .ForMember(dest => dest.TermandConditionIds, opt => opt.Ignore())
                .ForMember(dest => dest.PerformaItemIds, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalDocument, opt => opt.Ignore());

            // PerformaItemDetail mapping
            CreateMap<PerformaItemDetail, CarItemDetailDto>()
                .ForMember(dest => dest.MotorNo, opt => opt.MapFrom(src => src.MotorNo))
                .ForMember(dest => dest.ModelNo, opt => opt.MapFrom(src => src.ModelNo))
                .ForMember(dest => dest.ChassisNo, opt => opt.MapFrom(src => src.ChassisNo));

            // PerformaItem mapping
            CreateMap<PerformaItem, CarItemDto>()
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.PerformaItemDetails))
                .ForMember(dest => dest.CarId, opt => opt.MapFrom(src => src.CarId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Qunatity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));

            // CarPerforma to DTO mapping
            CreateMap<CarPerforma, GetCarPerformaDto>()
                .ForMember(dest => dest.cancellationPolicyIds, opt => opt.MapFrom(src => src.CancilationPolicyIds.Select(rp => rp.CancilationPolicyId)))
                .ForMember(dest => dest.paymentMethodIds, opt => opt.MapFrom(src => src.PaymentMethodIds.Select(rp => rp.PaymentMethodId)))
                .ForMember(dest => dest.specialInstructionIds, opt => opt.MapFrom(src => src.SpecialInstructionIds.Select(rp => rp.SpecialInstructionId)))
                .ForMember(dest => dest.termAndConditionIds, opt => opt.MapFrom(src => src.TermandConditionIds.Select(rp => rp.TermandConditionId)))
                .ForMember(dest => dest.companyAccountIds, opt => opt.MapFrom(src => src.CompanyAccountsIds.Select(rp => rp.CompanyAccountId)))
                // IDs
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.ToCompanyId, opt => opt.MapFrom(src => src.ToCompanyId))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer!.CustomerName))
                .ForMember(dest => dest.ToCompanyName, opt => opt.MapFrom(src => src.ToCompany != null ? src.ToCompany.Name : null))
                .ForMember(dest => dest.OnBehalfCompanyName, opt => opt.MapFrom(src => src.OnBehalfCompany != null ? src.OnBehalfCompany.Name : null))
                .ForMember(dest => dest.CarItems, opt => opt.MapFrom(src => src.PerformaItemIds))
                .ForMember(dest => dest.ApprovalDocument, opt => opt.MapFrom(src => src.ApprovalDocument));




            //RolePermission
            CreateMap<CreateRolePermissionDto, RolePermission>();
            CreateMap<UpdateRolePermissionDto, RolePermission>();
            CreateMap<RolePermission, GetRolePermissionDto>()
                 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.PermissionTitle, opt => opt.MapFrom(src => src.Permission!.PermissionTitle))
                 .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role!.Name));

            // TaskController
            CreateMap<TaskController, GetTaskControllerDto>();

            // Permission
            CreateMap<Permission, GetPermissionDto>();
            // Controller Action
            CreateMap<ControllerAction, GetControllerActionDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ActionName, opt => opt.MapFrom(src => src.ActionTask.ActionName))
                .ForMember(dest => dest.DisplayController, opt => opt.MapFrom(src => src.TaskController.DisplayController));
            // Task Action
            CreateMap<ActionTask, GetTaskActionDto>();

            //supplier
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();
            CreateMap<Supplier, GetSupplierDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.SupplierTypeName, opt => opt.MapFrom(src => src.SupplierType.TypeName));
            //// Product mappings
            //CreateMap<CreateProduct, Product>()
            //    .ForMember(dest => dest.Images, opt => opt.Ignore()) // Ignore Images
            //    .ForMember(dest => dest.Thumbnail, opt => opt.Ignore()) // Ignore Thumbnail
            //    .ForMember(dest => dest.ProductAttributes, opt => opt.MapFrom(src => src.ProductAttributes))
            //    .ForMember(dest => dest.ProductVariations, opt => opt.MapFrom(src => src.ProductVariations));
            //CreateMap<UpdateProduct, Product>()
            //    .ForMember(dest => dest.Images, opt => opt.Ignore()) // Ignore Images
            //    .ForMember(dest => dest.Thumbnail, opt => opt.Ignore()); // Ignore Thumbnail
            //CreateMap<Product, GetProduct>()
            //        .ForMember(dest => dest.ProductAttributes, opt => opt.MapFrom(src => src.ProductAttributes ?? new List<ProductAttribute>()))
            //        .ForMember(dest => dest.ProductVariations, opt => opt.MapFrom(src => src.ProductVariations ?? new List<ProductVariation>()))
            //       .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images ?? new List<string>()));



            // User mappings
            CreateMap<CreateUser, AppUser>();
            CreateMap<UpdateUser, AppUser>();
            CreateMap<LoginUser, AppUser>();
            CreateMap<AppUser, UserResponse>()
                 .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company!.Name))
                 .ForMember(dest => dest.BrnachName, opt => opt.MapFrom(src => src.Branch!.BranchName));

            CreateMap<AppUser, GetUser>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => string.Join(", ", src.UserRoles.Select(ur => ur.Role!.Name))))
                 .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company!.Name))
                 .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch!.BranchName));


            CreateMap<VerifyEmail, AppUser>();

            CreateMap<ResetPassword, AppUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        }
    }
}
