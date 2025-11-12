
using slick.Domain.Entities.Identity;
using System.Security.Claims;

namespace slick.Domain.Interfaces.Authentication
{
    public interface IUserManagement
    {
        Task<bool> CreateUser(AppUser user);
        Task<bool> LoginUser(AppUser user);
        Task<AppUser?> GetUserByEmail(string email);
        Task<AppUser> GetUserById(string id);
        Task<int> RemoveUserByEmail(string email);
        Task<List<Claim>> GetUserClaims(string email);
        Task<bool> IsEmailConfirmed(string email);  // Check if the email is confirmed
        Task<bool> UpdateUsslickassword(AppUser user);
        Task<bool> UpdateUserEmailStatus(AppUser user);
        Task<bool> CheckUserVerification(AppUser user);
        Task<IEnumerable<AppUser>?> GetAllUsers();
        Task<bool> UpdateUser(AppUser user);
        Task<bool> AssignRoleToUser(string userId, string roleId);
        Task<IList<string>> GetUserRoles(string userId);
        Task<bool> RemoveUserRole(string userId, string roleId);
        Task<List<AppUser>> GetUsersByRoleIdAsync(string roleId);
    }
}
