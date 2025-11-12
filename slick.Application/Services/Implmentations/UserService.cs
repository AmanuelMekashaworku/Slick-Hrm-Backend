using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Identity;
using slick.Application.Services.Interfaces;
using slick.Domain.Interfaces;
using slick.Domain.Interfaces.Authentication;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class UserService(IGeneric<AppUser> userRepository, IRoleManagement roleManagement,IBackgroundJobService backgroundJobService, IUserManagement userManagement,  IMapper mapper) : IUserService
    {
      

        public async Task<ServiceResponse> DeleteUserAsync(string id, string userId)
        {
            try
            {
                var result = await userRepository.DeleteByStringIdAsync(id, userId);
                return result > 0
                    ? new ServiceResponse(true, "User deleted successfully.")
                    : new ServiceResponse(false, "User not found or could not be deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while deleting Term and Condition.");
            }
        }
        public async Task<int> MultiSoftDeleteUserAsync(List<string> ids, string userId, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (!Guid.TryParse(userId, out var parsedUserId)) throw new ArgumentException("Invalid User ID format", nameof(userId));

            try
            {
                return await userRepository.MultiSoftDeleteByStringIdAsync(ids, parsedUserId, cancellationToken);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("ID property") || ex.Message.Contains("IsDeleted property"))
            {
                throw new InvalidOperationException("The Term and Condition entity doesn't support soft delete operations", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to soft delete Term and Condition", ex);
            }
        }
        public async Task<List<GetUser>> GetTrashedUserAsync(string? search)
        {
            try
            {
                Expression<Func<AppUser, bool>> filter = x => true;

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    filter = x =>
                    x.FirstName.ToLower().Contains(searchLower) ||
                    x.LastName.ToLower().Contains(searchLower) ||
                    x.EmployementIdNumber!.ToLower().Contains(searchLower) ||
                    x.Email!.ToLower().Contains(searchLower);
                }

                var appuser = await userRepository.GetTrashedAsync(filter);
                var appusersDTOs = mapper.Map<List<GetUser>>(appuser);

                // We'll store branches that should be deleted
                var toDelete = appusersDTOs.Where(b => b.DaysUntilHardDelete == 0).ToList();

                // Enqueue background jobs for them
                foreach (var appusersDTO in toDelete)
                {
                    backgroundJobService.Enqueue<IUserService>(x => x.HardDeleteUserAsync(appusersDTO.Id!));
                }

                // Remove them from the returned list
                appusersDTOs.RemoveAll(b => b.DaysUntilHardDelete == 0);

                return appusersDTOs;
            }
            catch
            {
                return new List<GetUser>();
            }
        }
        public async Task<bool> RecoverUserAsync(string id, CancellationToken cancellationToken = default)
        {
            int result = await userRepository.RecoverByStringIdAsync(id, cancellationToken);
            return result > 0;
        }
        public async Task<int> MultiRecoverUserAsync(List<string> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            return await userRepository.MultiRecoverByStringIdAsync(ids, cancellationToken);
        }
        //public async Task<ServiceResponse> HardDeleteUserAsync(string id)
        //{
        //    try
        //    {
        //        var result = await userRepository.HardDeleteByStringIdAsync(id);
        //        return result > 0
        //            ? new ServiceResponse(true, "User permanently deleted successfully.")
        //            : new ServiceResponse(false, "User  not found or could not be permanently deleted.");
        //    }
        //    catch
        //    {
        //        return new ServiceResponse(false, "Exception occurred while permanently deleting User.");
        //    }
        //}

        //public async Task<int> MultiHardDeleteUserAsync(List<String> ids, CancellationToken cancellationToken = default)
        //{
        //    return await userRepository.MultiHardDeleteByStringIdAsync(ids, cancellationToken);
        //}
        public async Task<ServiceResponse> HardDeleteUserAsync(string id)
        {
            try
            {
                // Step 1: Get the user
                var user = await userManagement.GetUserById(id);
                if (user == null)
                    return new ServiceResponse(false, "User not found.");

                // Step 2: Get all roles of the user as a string
                var userRolesString = await roleManagement.GetRoleIdByUserId(user.Id); // returns a string
                List<string> userRoles = new List<string>();

                if (!string.IsNullOrEmpty(userRolesString))
                {
                    // Split the roles string into individual role IDs
                    userRoles = userRolesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(r => r.Trim())
                                               .ToList();
                }

                // Step 3: Remove each role
                foreach (var roleId in userRoles)
                {
                    var result = await userManagement.RemoveUserRole(user.Id, roleId);
                    if (!result)
                    {
                        return new ServiceResponse(false, $"Failed to remove role with ID {roleId} from user.");
                    }
                }

                // Step 4: Delete the user
                var deleteResult = await userRepository.HardDeleteByStringIdAsync(id);

                return deleteResult > 0
                    ? new ServiceResponse(true, "User permanently deleted successfully.")
                    : new ServiceResponse(false, "User could not be permanently deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while permanently deleting User.");
            }
        }
        public async Task<int> MultiHardDeleteUserAsync(List<string> ids, CancellationToken cancellationToken = default)
        {
            foreach (var id in ids)
            {
                await HardDeleteUserAsync(id);
            }
            return ids.Count;
        }
        public async Task<int> GetDeletedUserCountAsync()
        {
            return await userRepository.CountDeletedAsync();
        }
        //public async Task<List<GetUser>> GetPagedUserAsync(string? search)
        //{
        //    try
        //    {
        //        // Initialize users collection
        //        IEnumerable<AppUser> users = Enumerable.Empty<AppUser>();

        //        // Get users based on search criteria
        //        if (string.IsNullOrWhiteSpace(search))
        //        {
        //            users = await userRepository.GetAllAsync() ?? Enumerable.Empty<AppUser>();
        //        }
        //        else
        //        {
        //            var searchProperties = new List<Expression<Func<AppUser, string>>>
        //    {
        //        x => x.FirstName,
        //        x => x.LastName!,
        //        x => x.PhoneNumber!
        //    };

        //            users = await userRepository.GetPagedAsync(
        //                search,
        //                x => x.IsActive && !x.IsDeleted,
        //                searchProperties
        //            ) ?? Enumerable.Empty<AppUser>();
        //        }

        //        // Map users to DTOs and include RoleId
        //        var result = new List<GetUser>();
        //        foreach (var user in users)
        //        {
        //            var userDto = mapper.Map<GetUser>(user);

        //            // Safe null handling for role ID
        //            if (user.Id != null)
        //            {
        //                var roleId = await roleManagement.GetRoleIdByUserId(user.Id);
        //                userDto.RoleId = roleId ?? string.Empty; // Or use null if your DTO allows it
        //            }
        //            else
        //            {
        //                userDto.RoleId = string.Empty;
        //            }

        //            result.Add(userDto);
        //        }

        //        return result;
        //    }
        //    catch
        //    {
        //        // Removed unused 'ex' variable since we're not logging it
        //        return new List<GetUser>();
        //    }
        //}
        public async Task<GetUser?> GetUserByIdAsync(string id)
        {
            try
            {
                var data = await userRepository.GetByStringIdAsync(id);
                return data is null ? null : mapper.Map<GetUser>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetUser>> GetPagedUserAsync(string? search)
        {
            try
            {
                // Include nullable navigation properties
                var includes = new Expression<Func<AppUser, object>>[]
                {
            x => x.Company!,
            x => x.Branch!,
                };

                IEnumerable<AppUser> users;

                if (string.IsNullOrWhiteSpace(search))
                {
                    users = await userRepository.GetPagedAsync(
                        search: null,
                        baseFilter: x => x.IsActive && !x.IsDeleted,
                        searchProperties: null,
                        cancellationToken: default,
                        includes: includes
                    ) ?? Enumerable.Empty<AppUser>();
                }
                else
                {
                    var searchProperties = new List<Expression<Func<AppUser, string>>>
            {
                x => x.FirstName,
                x => x.LastName!,
                x => x.PhoneNumber!
            };

                    users = await userRepository.GetPagedAsync(
                        search,
                        x => !x.IsDeleted,
                        searchProperties,
                        default,
                        includes
                    ) ?? Enumerable.Empty<AppUser>();
                }

                var result = new List<GetUser>();
                foreach (var user in users)
                {
                    var userDto = mapper.Map<GetUser>(user);

                    // Null-safe navigation property mapping
                    userDto.CompanyName = user.Company?.Name ?? string.Empty;
                    userDto.BranchName = user.Branch?.BranchName ?? string.Empty;

                    // Role mapping
                    if (user.Id != null)
                    {
                        var roleId = await roleManagement.GetRoleIdByUserId(user.Id);
                        userDto.RoleId = roleId ?? string.Empty;
                    }
                    else
                    {
                        userDto.RoleId = string.Empty;
                    }

                    result.Add(userDto);
                }

                return result;
            }
            catch
            {
                return new List<GetUser>();
            }
        }
        public async Task<IEnumerable<AppUser>> GetUserNamesAsync(CancellationToken cancellationToken = default)
        {
            try
            {

                var results = await userRepository.GetPropertyValuesAsync<AppUser>(
                    selectExpression: e => new AppUser
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                    },
                    cancellationToken: cancellationToken);

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
