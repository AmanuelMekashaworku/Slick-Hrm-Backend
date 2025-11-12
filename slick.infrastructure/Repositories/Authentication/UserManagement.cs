using slick.Application.DTOs;
using slick.Domain.Entities;
using slick.Domain.Entities.Identity;
using slick.Domain.Interfaces;
using slick.Domain.Interfaces.Authentication;
using slick.infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace slick.infrastructure.Repositories.Authentication
{
    public class UserManagement(IRoleManagement roleManagement, UserManager<AppUser> userManager, AppDbContext context) : IUserManagement
    {
        public async Task<bool> CreateUser(AppUser user)
        {
            var _user = await GetUserByEmail(user.Email!);
            if (_user != null) return false;

            return (await userManager.CreateAsync(user!, user!.PasswordHash!)).Succeeded;
        }
        public async Task<IEnumerable<AppUser>?> GetAllUsers() => await context.Users.ToListAsync();
        public async Task<AppUser?> GetUserByEmail(string email) => await userManager.FindByEmailAsync(email);
        public Task<AppUser> GetUserById(string id)
        {
            var user = userManager.FindByIdAsync(id);
            return user!;
        }

        public async Task<List<Claim>> GetUserClaims(string email)
        {
            var _user = await GetUserByEmail(email);
            string? roleName = await roleManagement.GetUserRole(_user!.Email!);
            List<Claim> claims = [
                new Claim("Firstname", _user!.FirstName),
                new Claim("Lastname", _user!.LastName),
                new Claim(ClaimTypes.NameIdentifier, _user!.Id),
                new Claim(ClaimTypes.Email, _user!.Email!),
                new Claim(ClaimTypes.Role, roleName!),
                ];
            return claims;
        }
        public async Task<bool> LoginUser(AppUser user)
        {
            var _user = await GetUserByEmail(user.Email!);
            if (_user is null) return false;

            string? roleName = await roleManagement.GetUserRole(_user!.Email!);
            if (string.IsNullOrEmpty(roleName)) return false;

            return await userManager.CheckPasswordAsync(_user, user.PasswordHash!);

        }
        public async Task<bool> CheckUserVerification(AppUser user)
        {
            var _user = await GetUserByEmail(user.Email!);
            if (_user is null) return false;

            if (_user.EmailConfirmed != true)
            {
                return false;
            }
            else return true;

        }
        public async Task<int> RemoveUserByEmail(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(_ => _.Email == email);
            context.Users.Remove(user!);
            return await context.SaveChangesAsync();
        }
        public async Task<bool> UpdateUsslickassword(AppUser user)
        {
            context.Users.Update(user);
            return await context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateUserEmailStatus(AppUser user)
        {
            context.Users.Update(user);
            return await context.SaveChangesAsync() > 0;
        }
        public async Task<bool> IsEmailConfirmed(string email)
        {
            var user = await GetUserByEmail(email);
            return user?.EmailConfirmed ?? false;
        }
        public async Task<bool> UpdateUser(AppUser user)
        {
            var dbUser = await userManager.FindByIdAsync(user.Id);
            if (dbUser == null)
                return false;

            // Update fields
            dbUser.FirstName = user.FirstName;
            dbUser.LastName = user.LastName;
            dbUser.UserName = user.UserName;
            dbUser.PhoneNumber = user.PhoneNumber;

            var result = await userManager.UpdateAsync(dbUser);
            return result.Succeeded;
        }
        public async Task<bool> AssignRoleToUser(string userId, string roleId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var role = await roleManagement.GetRoleById(roleId);
            if (role == null || string.IsNullOrEmpty(role.Name)) return false;

            var result = await userManager.AddToRoleAsync(user, role.Name);
            return result.Succeeded;
        }
        public async Task<IList<string>> GetUserRoles(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return await userManager.GetRolesAsync(user);
        }
        public async Task<bool> RemoveUserRole(string userId, string roleId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Get role name by roleId from roleManagement
            var role = await roleManagement.GetRoleById(roleId);
            if (role == null || string.IsNullOrEmpty(role.Name))
                return false;

            var result = await userManager.RemoveFromRoleAsync(user, role.Name);
            return result.Succeeded;
        }
        public async Task<List<AppUser>> GetUsersByRoleIdAsync(string roleId)
        {
            try
            {
                // Get the role name from roleId using your roleManagement
                var role = await roleManagement.GetRoleById(roleId);
                if (role == null || string.IsNullOrEmpty(role.Name))
                    return new List<AppUser>();

                // Get all users in that role
                var usersInRole = await userManager.GetUsersInRoleAsync(role.Name);
                return usersInRole.ToList();
            }
            catch (Exception )
            {
                // Log the exception here
                return new List<AppUser>();
            }
        }

    }
}



