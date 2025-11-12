using slick.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using slick.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace slick.Domain.Interfaces.Authentication
{
    public interface IRoleManagement
    {
        Task<bool> AddUserToRole(AppUser user, string roleName);
        Task<bool> CreateRole(string roleName, string description, bool isEditable);
        Task<IdentityRole?> FindRoleByNameAsync(string roleName);
        Task<ApplicationRole?> GetRoleById(string roleId);
        Task<ApplicationRole?> GetRoleByNameAsync(string roleName);
        Task<string?> GetRoleIdByUserId(string userId);
        Task<string?> GetUserRole(string userEmail);
        Task<IList<string>> GetUserRoles(AppUser user);
        Task<bool> UpdateRoleById(string roleId, string roleName, string description, bool isEditable);
    }
}