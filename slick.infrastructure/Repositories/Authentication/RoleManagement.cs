using slick.Application.DTOs.RolePermission;
using slick.Domain.Entities.Identity;
using slick.Domain.Interfaces.Authentication;
using Microsoft.AspNetCore.Identity;
using SMS.Domain.Models;
using System.Threading.Tasks;

namespace slick.Infrastructure.Authentication
{
    public class RoleManagement(UserManager<AppUser> userManager, RoleManager<ApplicationRole> roleManager) : IRoleManagement
    {
        public async Task<bool> AddUserToRole(AppUser user, string roleName) =>
            (await userManager.AddToRoleAsync(user, roleName)).Succeeded;

        public async Task<string?> GetUserRole(string userEmail)
        {
            var user = await userManager.FindByEmailAsync(userEmail);
            return (await userManager.GetRolesAsync(user!)).FirstOrDefault();
        }

        public async Task<IList<string>> GetUserRoles(AppUser user) =>
            await userManager.GetRolesAsync(user);

        public async Task<bool> CreateRole(string roleName, string description, bool isEditable)
        {
            // Create the Identity role
            var result = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpper(),
                Description = description,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow
            });

            return result.Succeeded;
        }

        public async Task<bool> UpdateRoleById(string roleId, string roleName, string description, bool isEditable)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null) return false;

            role.Name = roleName;
            role.NormalizedName = roleName.ToUpper();
            role.Description = description;
            role.LastModifiedDate = DateTime.UtcNow;

            var result = await roleManager.UpdateAsync(role);
            return result.Succeeded;
        }
        public async Task<IdentityRole?> FindRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return null;

            return await roleManager.FindByNameAsync(roleName);
        }

        public async Task<ApplicationRole?> GetRoleById(string roleId) =>
            await roleManager.FindByIdAsync(roleId);
            
        public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName)
        {
            return await roleManager.FindByNameAsync(roleName);
        }
        public async Task<string?> GetRoleIdByUserId(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return null;

            // Get the user's role name (assuming single role)
            var roleName = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            if (roleName == null) return null;

            // Fetch the role ID by name
            var role = await roleManager.FindByNameAsync(roleName);
            return role?.Id;
        }

    }
}