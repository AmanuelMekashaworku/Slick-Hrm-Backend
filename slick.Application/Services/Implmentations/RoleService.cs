using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Role;
using slick.Application.DTOs.RolePermission;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using slick.Domain.Interfaces.Authentication;
using Microsoft.AspNetCore.Cors.Infrastructure;
using slick.Domain.Models;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class RoleService(IGeneric<ApplicationRole> roleRepository,IRolePermissionService rolePermissionService,IUserManagement userManagement,IBackgroundJobService backgroundJobService,IRoleManagement roleManagement,  IMapper mapper) : IRoleService
    {

        public async Task<ServiceResponse> CreateRoleAsync(CreateRoleDto roleDto)
        {
            try
            {
                var entity = mapper.Map<ApplicationRole>(roleDto);
                entity.IsActive = true;
                entity.IsDeleted = false;
                entity.NormalizedName=roleDto.Name.ToUpper();
                entity.RolePermissions = new List<RolePermission>();

              
                if (roleDto.PermissionIds != null && roleDto.PermissionIds.Any())
                {
                    foreach (var permissionId in roleDto.PermissionIds)
                    {
                        entity.RolePermissions.Add(new RolePermission
                        {
                            RoleId = entity.Id,
                            PermissionId = permissionId
                        });
                    }
                }
                var exists = await roleRepository.ExistsAsync("Name", entity.Name!);
                if (exists)
                {
                    throw new InvalidOperationException("A role with this name already exists.");
                }

                var result = await roleRepository.AddAsync(entity);

                return result > 0
                    ? new ServiceResponse(true, "Role created successfully.")
                    : new ServiceResponse(false, "Failed to create Role.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Error on creating Role: {ex.Message}");
            }
        }
        public async Task<ServiceResponse> AddRolePermissionAsync(AddRolePermissionDto dto)
        {
            try
            {
                if (dto.PermissionIds == null || !dto.PermissionIds.Any())
                {
                    return new ServiceResponse(false, "No permissions provided.");
                }
                if (string.IsNullOrEmpty(dto.RoleId))
                {
                    return new ServiceResponse(false, "Role ID cannot be empty.");
                }

                var role = await roleManagement.GetRoleById(dto.RoleId);
                if (role == null)
                {
                    return new ServiceResponse(false, "Role not found.");
                }

                role.RolePermissions ??= new List<RolePermission>();

                foreach (var permissionId in dto.PermissionIds)
                {
                    // Prevent duplicate permission entries
                    if (!role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
                    {
                        role.RolePermissions.Add(new RolePermission
                        {
                            RoleId = dto.RoleId,
                            PermissionId = permissionId
                        });
                    }
                }

                var result = await roleRepository.UpdateAsync(role);

                return result > 0
                    ? new ServiceResponse(true, "Permissions added to role successfully.")
                    : new ServiceResponse(false, "Failed to add permissions to role.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Exception occurred while adding permissions: {ex.Message}");
            }
        }
        //public async Task<ServiceResponse> UpdateRoleAsync(UpdateRoleDto roleDto)
        //{
        //    try
        //    {
        //        // 1️⃣ Update in Identity's AspNetRoles
        //        var identityUpdated = await roleManagement.UpdateRoleById(
        //            roleDto.ID.ToString(),
        //            roleDto.Name,// this must be the Id from AspNetRoles
        //            roleDto.Description ?? string.Empty,
        //            roleDto.Editable ?? false
        //        );

        //        if (!identityUpdated)
        //            return new ServiceResponse(false, "Failed to update Identity role.");

        //        // 2️⃣ Update in your own Role table
        //        var existing = await roleManagement.GetRoleById(roleDto.ID);
        //        if (existing is null)
        //            return new ServiceResponse(false, "Role not found.");

        //        mapper.Map(roleDto, existing);
        //        existing.LastModifiedDate = DateTime.Now;
        //        // Convert ModifiedBy to Guid
        //        if (!string.IsNullOrEmpty(roleDto.ModifiedBy) && Guid.TryParse(roleDto.ModifiedBy, out Guid modifiedByGuid))
        //        {
        //            existing.ModifiedBy = modifiedByGuid;
        //        }
        //        else
        //        {
        //            throw new Exception("Invalid ModifiedBy ID format");
        //        }

        //        existing.RolePermissions.Clear();
        //        if (roleDto.PermissionIds != null)
        //        {
        //            foreach (var pid in roleDto.PermissionIds)
        //            {
        //                existing.RolePermissions.Add(new RolePermission
        //                {
        //                    RoleId = existing.Id,
        //                    PermissionId = pid
        //                });
        //            }
        //        }
        //        var result = await roleRepository.UpdateAsync(existing);

        //        return result > 0
        //            ? new ServiceResponse(true, "Role updated successfully.")
        //            : new ServiceResponse(false, "Failed to update Role in application DB.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse(false, $"Exception occurred while updating Role: {ex.Message}");
        //    }
        //}
        public async Task<ServiceResponse> UpdateRoleAsync(UpdateRoleDto roleDto)
        {
            try
            {
                // 1️⃣ Update in Identity's AspNetRoles
                var identityUpdated = await roleManagement.UpdateRoleById(
                    roleDto.ID.ToString(),
                    roleDto.Name,
                    roleDto.Description ?? string.Empty,
                    roleDto.Editable ?? false
                );

                if (!identityUpdated)
                    return new ServiceResponse(false, "Failed to update Identity role.");

                // 2️⃣ Update in your own Role table
                var existing = await roleManagement.GetRoleById(roleDto.ID);
                if (existing is null)
                    return new ServiceResponse(false, "Role not found.");

                // Clear existing permissions using generic method
                await roleRepository.ClearChildCollectionAsync(
                    roleDto.ID,
                    r => r.RolePermissions
                );

                // Add new permissions
                if (roleDto.PermissionIds != null)
                {
                    foreach (var pid in roleDto.PermissionIds)
                    {
                        existing.RolePermissions.Add(new RolePermission
                        {
                            RoleId = existing.Id,
                            PermissionId = pid
                        });
                    }
                }

                mapper.Map(roleDto, existing);
                existing.LastModifiedDate = DateTime.Now;

                if (!string.IsNullOrEmpty(roleDto.ModifiedBy))
                {
                    existing.ModifiedBy = roleDto.ModifiedBy;
                }
                else
                {
                    throw new Exception("Invalid ModifiedBy ID format");
                }

                var result = await roleRepository.UpdateAsync(existing);

                return result > 0
                    ? new ServiceResponse(true, "Role updated successfully.")
                    : new ServiceResponse(false, "Failed to update Role in application DB.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Exception occurred while updating Role: {ex.Message}");
            }
        }
        public async Task<GetRoleDto?> GetRoleByIdAsync(Guid id)
        {
            try
            {
                var data = await roleRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetRoleDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetRoleDto>> GetAllRolesAsync()
        {
            try
            {
                var roles = await roleRepository.GetAllAsync();
                return mapper.Map<List<GetRoleDto>>(roles);
            }
            catch
            {
                return new List<GetRoleDto>();
            }
        }
        public async Task<List<GetRoleDto>> GetPagedRoleAsync(string? search)
        {
            try
            {
                // Define includes for navigation properties
                var includes = new Expression<Func<ApplicationRole, object>>[]
                {
            r => r.RolePermissions! // Include role permissions
                };

                List<ApplicationRole> roles;

                if (string.IsNullOrWhiteSpace(search))
                {
                    roles = await roleRepository.GetPagedAsync(
                        search: null,
                        baseFilter: x => x.IsActive && !x.IsDeleted,
                        searchProperties: null,
                        cancellationToken: default,
                        includes: includes // Add includes here
                    );
                }
                else
                {
                    var searchProperties = new List<Expression<Func<ApplicationRole, string>>>
            {
                x => x.Name!,
                x => x.Description!
            };

                    roles = await roleRepository.GetPagedAsync(
                        search,
                        x => x.IsActive && !x.IsDeleted,
                        searchProperties,
                        default,
                        includes // Add includes here
                    );
                }

                // Debugging: Check if roles are being retrieved
                if (roles == null || !roles.Any())
                {
                    // Log or handle empty roles case
                    return new List<GetRoleDto>();
                }

                var roleDtos = mapper.Map<List<GetRoleDto>>(roles);

                // Debugging: Check mapping results
                if (roleDtos == null || !roleDtos.Any())
                {
                    // Log or handle mapping issue
                    return new List<GetRoleDto>();
                }

                // For each role, get its full permission details
                foreach (var roleDto in roleDtos)
                {
                    if (roleDto.ID == null)
                    {
                        throw new InvalidOperationException("role is not empty");
                    }
                    var permissions = await rolePermissionService.GetRolePermissionsByRoleIdAsync(roleDto.ID);

                    // Debugging: Check permissions retrieval
                    if (permissions != null && permissions.Any())
                    {
                        roleDto.Permissions = permissions;
                        roleDto.PermissionIds = permissions.Select(p => p.PermissionId).ToList();
                    }
                    else
                    {
                        roleDto.Permissions = new List<GetRolePermissionDto>();
                        roleDto.PermissionIds = new List<Guid>();
                    }
                }

                return roleDtos;
            }
            catch (Exception)
            {
                // Add proper logging here
                // _logger.LogError(ex, "Error getting paged roles");
                return new List<GetRoleDto>();
            }
        }
        public async Task<ServiceResponse> DeleteRoleAsync(String id, string userId)
        {
            try
            {
                var result = await roleRepository.DeleteByStringIdAsync(id, userId);
                return result > 0
                    ? new ServiceResponse(true, "Role deleted successfully.")
                    : new ServiceResponse(false, "Role not found or could not be deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while deleting Role.");
            }
        }
        public async Task<int> MultiSoftDeleteRoleAsync(List<String> ids, string userId, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (!Guid.TryParse(userId, out var parsedUserId)) throw new ArgumentException("Invalid User ID format", nameof(userId));


            try
            {
                return await roleRepository.MultiSoftDeleteByStringIdAsync(ids, parsedUserId, cancellationToken);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ID property") || ex.Message.Contains("IsDeleted property"))
            {
                throw new InvalidOperationException("The role entity doesn't support soft delete operations", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to soft delete Car", ex);
            }
           
        }
        public async Task<List<GetRoleDto>> GetTrashedRoleAsync(string? search)
        {
            try
            {
                Expression<Func<ApplicationRole, bool>> filter = x => true;

                if (!string.IsNullOrEmpty(search))
                {
                    string searchLower = search.ToLower();
                    filter = x =>
                        x.Name!.ToLower().Contains(searchLower) ||
                        (x.Description != null && x.Description!.ToLower().Contains(searchLower));
                }

                var roles = await roleRepository.GetTrashedAsync(filter);
                var roleDTOs = mapper.Map<List<GetRoleDto>>(roles);

                // We'll store branches that should be deleted
                var toDelete = roleDTOs.Where(b => b.DaysUntilHardDelete == 0).ToList();

                // Enqueue background jobs for them
                foreach (var roleDTO in toDelete)
                {
                    backgroundJobService.Enqueue<IRoleService>(x => x.HardDeleteRoleAsync(roleDTO.ID!));
                }

                // Remove them from the returned list
                roleDTOs.RemoveAll(b => b.DaysUntilHardDelete == 0);

                return roleDTOs;
            }
            catch
            {
                return new List<GetRoleDto>();
            }
        }
        public async Task<bool> RecoverRoleAsync(String id, CancellationToken cancellationToken = default)
        {
            int result = await roleRepository.RecoverByStringIdAsync(id, cancellationToken);
            return result > 0;
        }
        public async Task<int> MultiRecoverRolesAsync(List<String> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            return await roleRepository.MultiRecoverByStringIdAsync(ids, cancellationToken);
        }
        public async Task<ServiceResponse> HardDeleteRoleAsync(String id)
        {
            try
            {
                var result = await roleRepository.HardDeleteonDeleteCascadeofstringAsync(id);
                return result > 0
                    ? new ServiceResponse(true, "Role permanently deleted successfully.")
                    : new ServiceResponse(false, "Role not found or could not be permanently deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while permanently deleting Role.");
            }
        }
        public async Task<int> MultiHardDeleteRolesAsync(List<String> ids, CancellationToken cancellationToken = default)
        {
            return await roleRepository.MultiHardDeleteByStringIdAsync(ids, cancellationToken);
        }
        public async Task<int> GetDeletedRolesCountAsync()
        {
            return await roleRepository.CountDeletedAsync();
        }
        public async Task<IEnumerable<GetRoleNameDto>> GetRoleNamesAsync(CancellationToken cancellationToken = default)
        {
            try
            {

                var results = await roleRepository.GetPropertyValuesAsync<GetRoleNameDto>(
                    selectExpression: e => new GetRoleNameDto
                    {
                        id = e.Id,
                        Name = e.Name!
                    },
                    cancellationToken: cancellationToken);

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ServiceResponse> DeleteRolePermissionAsync(Guid id)
        {
            try
            {
                var result = await rolePermissionService.HardDeleteRolePermissionAsync(id);
                return result.Success
                    ? new ServiceResponse(true, "Role Permission deleted successfully.")
                    : new ServiceResponse(false, result.Message);
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while deleting Role Permission.");
            }
        }
        public async Task<List<string>> GetUserIdsByPermissionTitleAsync(string permissionTitle)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionTitle))
                    return new List<string>();

                // Get all roles using your existing method
                var roles = await GetPagedRoleAsync(null);

                if (roles == null || !roles.Any())
                    return new List<string>();

                var userIds = new List<string>();

                foreach (var role in roles)
                {
                    // Check if this role has the permission title we're looking for
                    if (role.Permissions != null && role.Permissions.Any(p =>
                        p.PermissionTitle?.Equals(permissionTitle, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        if (!string.IsNullOrEmpty(role.ID))
                        {
                            // Get users for this role and add their IDs
                            var usersInRole = await userManagement.GetUsersByRoleIdAsync(role.ID);
                            if (usersInRole != null && usersInRole.Any())
                            {
                                userIds.AddRange(usersInRole.Select(u => u.Id));
                            }
                        }
                    }
                }

                // Remove duplicates in case a user has multiple roles with same permission
                return userIds.Distinct().ToList();
            }
            catch (Exception)
            {
                // _logger.LogError(ex, $"Error getting user IDs for permission title: {permissionTitle}");
                return new List<string>();
            }
        }
    }
}
