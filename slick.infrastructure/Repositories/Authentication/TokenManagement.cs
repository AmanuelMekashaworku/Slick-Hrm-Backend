using slick.Domain.Entities.Identity;
using slick.Domain.Interfaces.Authentication;
using slick.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slick.Infrastructure.Repositories.Authentication
{
    public class TokenManagement : ITokenManagement
    {
        private readonly AppDbContext context;
        private readonly IConfiguration config;
        private readonly string _jwtSecret;

        public TokenManagement(AppDbContext context, IConfiguration config)
        {
            this.context = context;
            this.config = config;
            _jwtSecret = config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key is not configured");
        }

        public async Task<int> AddRefreshToken(string userId, string refreshToken)
        {
            context.RefreshTokens.Add(new RefreshToken { UserId = userId, Token = refreshToken });
            return await context.SaveChangesAsync();
        }

        public string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // Set to 1 day to match jwt cookie
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetRefreshToken()
        {
            const int byteSize = 64;
            byte[] randomBytes = new byte[byteSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<string?> GetEmailFromVerificationToken(string token)
        {
            var tokenEntry = await context.VerificationTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (tokenEntry == null || tokenEntry.ExpiryDate < DateTime.UtcNow)
                return null;

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == tokenEntry.UserId);
            return user?.Email;
        }

        public List<Claim> GetUserClaims(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken?.Claims.ToList() ?? new List<Claim>();
        }

        public async Task<string?> GetUserIdByRefreshToken(string refreshToken)
        {
            var tokenEntry = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
            return tokenEntry?.UserId;
        }

        public async Task<int> UpdateRefreshToken(string userId, string refreshToken)
        {
            var tokenEntry = await context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tokenEntry == null)
                return -1;

            tokenEntry.Token = refreshToken;
            return await context.SaveChangesAsync();
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            var tokenEntry = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
            return tokenEntry != null;
        }

        public async Task<int> AddVerificationToken(string userId, string verificationToken)
        {
            int tokenValidityMinutes = config.GetValue<int>("Jwt:TokenValidityMinutes", 10);
            var token = new VerificationToken
            {
                UserId = userId,
                Token = verificationToken,
                ExpiryDate = DateTime.UtcNow.AddMinutes(tokenValidityMinutes)
            };

            context.VerificationTokens.Add(token);
            return await context.SaveChangesAsync();
        }

        public async Task<bool> ValidateVerificationToken(string verificationToken)
        {
            var token = await context.VerificationTokens.FirstOrDefaultAsync(t => t.Token == verificationToken);
            return token != null && token.ExpiryDate >= DateTime.UtcNow;
        }

        public async Task<bool> RemoveRefreshToken(string userId, string refreshToken)
        {
            var token = await context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken);
            if (token == null)
                return false;

            context.RefreshTokens.Remove(token);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRefreshTokenByUserId(string userId)
        {
            var tokens = await context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
            if (!tokens.Any())
                return false;

            context.RefreshTokens.RemoveRange(tokens);
            await context.SaveChangesAsync();
            return true;
        }

        public Task<string> GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                    throw new SecurityTokenException("User ID not found in token");

                return Task.FromResult(userId);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token", ex);
            }
        }
    }
}