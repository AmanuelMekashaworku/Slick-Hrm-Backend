using erp.Application.DTOs;
using erp.Application.DTOs.RolePermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IRolePermissionService
    {
        Task<ServiceResponse> CreateRolePermissionAsync(CreateRolePermissionDto rolePermissionDto);
        Task<List<GetRolePermissionDto>> GetAllRolePermissionsAsync();
        Task<int> GetDeletedRolePermissionsCountAsync();
        Task<List<GetRolePermissionDto>> GetPagedRolePermissionsAsync(string? search);
        Task<GetRolePermissionDto?> GetRolePermissionByIdAsync(Guid id);
        Task<List<GetRolePermissionDto>> GetRolePermissionsByRoleIdAsync(string roleId);
        Task<ServiceResponse> HardDeleteRolePermissionAsync(Guid id);
        Task<int> MultiHardDeleteRolePermissionsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    }
}
