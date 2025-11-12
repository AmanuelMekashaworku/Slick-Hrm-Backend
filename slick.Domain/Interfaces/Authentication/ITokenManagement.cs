using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace slick.Domain.Interfaces.Authentication
{
    public interface ITokenManagement
    {
        Task<int> AddRefreshToken(string userId, string refreshToken);
        Task<int> AddVerificationToken(string userId, string verificationToken);
        string GenerateToken(List<Claim> claims);
        string GetRefreshToken();
        List<Claim> GetUserClaims(string token);
        Task<string?> GetEmailFromVerificationToken(string token);
        Task<string?> GetUserIdByRefreshToken(string refreshToken);
        Task<string> GetUserIdFromToken(string token);
        Task<bool> RemoveRefreshToken(string userId, string refreshToken);
        Task<bool> RemoveRefreshTokenByUserId(string userId);
        Task<int> UpdateRefreshToken(string userId, string refreshToken);
        Task<bool> ValidateRefreshToken(string refreshToken);
        Task<bool> ValidateVerificationToken(string verificationToken);
    }
}