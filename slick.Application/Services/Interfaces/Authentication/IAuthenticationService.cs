using slick.Application.DTOs;
using slick.Application.DTOs.Identity;
using Microsoft.AspNetCore.Http;

namespace slick.Application.Services.Interfaces.Authentication
{
    public interface IAuthenticationService
    {
        Task<ServiceResponse> CreateUser(CreateUser user);
        Task<LoginResponse> LoginUser(LoginUser user);
        Task<LoginResponse> ReviveToken(string refreshToken);
        Task<List<string>> GetUserRoles(string? userId);
        string HashPassword(string plainPassword);
        Task<ServiceResponse> ForgotPassword(string email, string newPassword);
        Task<ServiceResponse> SendVerificationEmail(string email);
        Task<ServiceResponse> VerifyToken(string verificationToken);
        Task<ServiceResponse> Logout(string token);
        Task<ServiceResponse> GetAllUsers();
        Task<ServiceResponse> GetUserByToken(string token);
        Task<ServiceResponse> UpdateUserByToken(string token, UpdateUser updateUser);
        Task<ServiceResponse> UpdateProfilePictureByToken(string token, IFormFile file);
        Task<ServiceResponse> UpdateUser(UpdateUser userDto);
        Task<ServiceResponse> UpdateEmail(string token, string newEmail);
    }
}