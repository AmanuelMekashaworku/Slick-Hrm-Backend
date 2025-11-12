using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.RolePermission;
using slick.Application.Services.Interfaces;
using slick.Domain.Interfaces;
using slick.Domain.Models;
using System.Linq.Expressions;

namespace slick.Application.Services.Implmentations
{
    public class RolePermissionService(IGeneric<RolePermission> rolePermissionRepository, IMapper mapper) : IRolePermissionService
    {
        public async Task<ServiceResponse> CreateRolePermissionAsync(CreateRolePermissionDto rolePermissionDto)
        {
            try
            {
                var entity = mapper.Map<RolePermission>(rolePermissionDto);
                entity.IsActive = true;
                entity.IsDeleted = false;
                var result = await rolePermissionRepository.AddAsync(entity);

                return result > 0
                    ? new ServiceResponse(true, "Role Permission created successfully.")
                    : new ServiceResponse(false, "Failed to create Role Permission.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while creating Role Permission.");
            }
        }
        public async Task<GetRolePermissionDto?> GetRolePermissionByIdAsync(Guid id)
        {
            try
            {
                var data = await rolePermissionRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetRolePermissionDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetRolePermissionDto>> GetRolePermissionsByRoleIdAsync(string roleId)
        {
            try
            {

                // Include Role and Permission navigation properties
                var includes = new Expression<Func<RolePermission, object>>[]
                {
            rp => rp.Role!,       // Include Role (nullable if needed)
            rp => rp.Permission!  // Include Permission (nullable if needed)
                };

                var rolePermissions = await rolePermissionRepository.GetByRelationalIdAsync(
                    foreignKeyPropertyName: "RoleId",
                    foreignKeyValue: roleId,
                    includes: includes
                );

                if (rolePermissions == null || !rolePermissions.Any())
                    return new List<GetRolePermissionDto>();

                return mapper.Map<List<GetRolePermissionDto>>(rolePermissions);
            }
            catch (Exception)
            {
                // Log error (uncomment if you have logging)
                // _logger.LogError(ex, "Error fetching role permissions for role {RoleId}", roleId);
                return new List<GetRolePermissionDto>();
            }
        }
        public async Task<List<GetRolePermissionDto>> GetAllRolePermissionsAsync()
        {
            try
            {
                var permissions = await rolePermissionRepository.GetAllAsync();
                return mapper.Map<List<GetRolePermissionDto>>(permissions);
            }
            catch
            {
                return new List<GetRolePermissionDto>();
            }
        }
        public async Task<List<GetRolePermissionDto>> GetPagedRolePermissionsAsync(string? search)
        {
            try
            {
                // Define includes for navigation properties
                var includes = new Expression<Func<RolePermission, object>>[]
                 {
                    x => x.Permission!, x => x.Role!
                 };
                if (string.IsNullOrWhiteSpace(search))
                {
                    var rolePermissions = await rolePermissionRepository.GetPagedAsync(
                        search: null,
                        baseFilter: x => !x.IsDeleted,
                        searchProperties: null,
                        cancellationToken: default,
                        includes: includes
                    );
                    var result = mapper.Map<List<GetRolePermissionDto>>(rolePermissions);
                    // Debugging: Check if result is empty
                    if (result == null || !result.Any())
                    {
                        Console.WriteLine("No active role permissions found or mapping failed.");
                    }
                    return result ?? new List<GetRolePermissionDto>();
                }

                var searchProperties = new List<Expression<Func<RolePermission, string>>>
        {
            x => x.Permission!.PermissionTitle,
            x => x.Role!.Name! // Added for consistency with frontend search
        };

                var filteredRolePermissions = await rolePermissionRepository.GetPagedAsync(
                    search,
                    x => x.IsActive && !x.IsDeleted,
                    searchProperties,
                    default,
                    includes
                );

                var mappedResult = mapper.Map<List<GetRolePermissionDto>>(filteredRolePermissions);
                // Debugging: Check if result is empty
                if (mappedResult == null || !mappedResult.Any())
                {
                    Console.WriteLine($"No role permissions found for search term: {search}");
                }
                return mappedResult ?? new List<GetRolePermissionDto>();
            }
            catch (Exception ex)
            {
                // Debugging: Output exception details
                Console.WriteLine($"Error in GetPagedRolePermissionsAsync: {ex.Message}");
                return new List<GetRolePermissionDto>();
            }
        }
        public async Task<int> GetDeletedRolePermissionsCountAsync()
        {
            return await rolePermissionRepository.CountDeletedAsync();
        }
        public async Task<ServiceResponse> HardDeleteRolePermissionAsync(Guid id)
        {
            try
            {
                var result = await rolePermissionRepository.HardDeleteAsync(id);
                return result > 0
                    ? new ServiceResponse(true, "Role  Permission permanently deleted successfully.")
                    : new ServiceResponse(false, "Role Permission not found or could not be permanently deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while permanently deleting Role.");
            }
        }
        public async Task<int> MultiHardDeleteRolePermissionsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await rolePermissionRepository.MultiHardDeleteAsync(ids, cancellationToken);
        }
        public async Task<bool> RecoverRolePermissionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            int result = await rolePermissionRepository.RecoverAsync(id, cancellationToken);
            return result > 0;
        }
        public async Task<int> MultiRecoverRolePermissionsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            return await rolePermissionRepository.MultiHardDeleteAsync(ids, cancellationToken);
        }

    }
}
