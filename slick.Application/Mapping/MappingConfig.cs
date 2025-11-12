    
using AutoMapper;
using slick.Application.DTOs.Branch;
using slick.Application.DTOs.BusinessGroup;
using slick.Application.DTOs.Chat;
using slick.Application.DTOs.Company;
using slick.Application.DTOs.ControllerAction;
using slick.Application.DTOs.Identity;
using slick.Application.DTOs.Permission;
using slick.Application.DTOs.RolePermission;
using slick.Application.DTOs.TaskAction;
using slick.Application.DTOs.TaskController;
using slick.Application.DTOs.UserLog;
using slick.Domain.Entities;
using slick.Domain.Models;

namespace slick.Application.Mapping
{
    internal class MappingConfig : Profile
    {
         public MappingConfig() 
        {

        
            //branch mappings
            CreateMap<CreateBranchDto, Branch>();
            CreateMap<UpdateBranchDto, Branch>();
            CreateMap<Branch, GetBranchDto>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name));
          
            //Business Group
            CreateMap<CreateBusinessGroupDto, BusinessGroup>();
            CreateMap<UpdateBusinessGroupDto, BusinessGroup>();
            CreateMap<BusinessGroup, GetBusinessGroupDto>();
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

           CreateMap<CreateUserActivityLogDto, UserActivityLog>()
              .ForMember(dest => dest.Id, opt => opt.Ignore()) 
              .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()); 

           CreateMap<UserActivityLog, UserActivityLogDto>();
            //chat mapping 
            CreateMap<CreateChatMessageDto, ChatMessage>();
            CreateMap<ChatMessage, ChatMessageDto>();

        }
    }
}
