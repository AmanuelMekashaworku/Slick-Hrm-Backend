using erp.Application.DTOs;
using erp.Application.DTOs.Role;
using erp.Application.DTOs.RolePermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IRoleService
    {
        Task<ServiceResponse> AddRolePermissionAsync(AddRolePermissionDto dto);
        Task<ServiceResponse> CreateRoleAsync(CreateRoleDto roleDto);
        Task<ServiceResponse> DeleteRoleAsync(string id, string userId);
        Task<ServiceResponse> DeleteRolePermissionAsync(Guid id);
        Task<List<GetRoleDto>> GetAllRolesAsync();
        Task<int> GetDeletedRolesCountAsync();
        Task<List<GetRoleDto>> GetPagedRoleAsync(string? search);
        Task<GetRoleDto?> GetRoleByIdAsync(Guid id);
        Task<IEnumerable<GetRoleNameDto>> GetRoleNamesAsync(CancellationToken cancellationToken = default);
        Task<List<GetRoleDto>> GetTrashedRoleAsync(string? search);
        Task<List<string>> GetUserIdsByPermissionTitleAsync(string permissionTitle);
        Task<ServiceResponse> HardDeleteRoleAsync(String id);
        Task<int> MultiHardDeleteRolesAsync(List<String> ids, CancellationToken cancellationToken = default);
        Task<int> MultiRecoverRolesAsync(List<String> ids, CancellationToken cancellationToken = default);
        Task<int> MultiSoftDeleteRoleAsync(List<String> ids, string userId, CancellationToken cancellationToken = default);
        Task<bool> RecoverRoleAsync(String id, CancellationToken cancellationToken = default);
        Task<ServiceResponse> UpdateRoleAsync(UpdateRoleDto roleDto);
    }
}
