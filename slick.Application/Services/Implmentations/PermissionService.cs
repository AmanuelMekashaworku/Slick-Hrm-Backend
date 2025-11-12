using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Permission;
using slick.Application.Services.Interfaces;
using SMS.Domain.Models;
using slick.Domain.Interfaces;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class PermissionService(IGeneric<Permission> permissionRepository, IMapper mapper) : IPermissionService
    {
        
        public async Task<GetPermissionDto?> GetPermissionByIdAsync(Guid id)
        {
            try
            {
                var data = await permissionRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetPermissionDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetPermissionDto>> GetAllPermissionsAsync()
        {
            try
            {
                var permissions = await permissionRepository.GetRoleAllAsync();
                return mapper.Map<List<GetPermissionDto>>(permissions);
            }
            catch
            {
                return new List<GetPermissionDto>();
            }
        }
       
               
    }
}
