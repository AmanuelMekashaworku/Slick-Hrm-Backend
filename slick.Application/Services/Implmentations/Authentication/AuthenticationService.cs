using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Identity;
using slick.Application.DTOs.Role;
using slick.Application.DTOs.RolePermission;
using slick.Application.Services.Implmentations;
using slick.Application.Services.Interfaces;
using slick.Application.Services.Interfaces.Authentication;
using slick.Application.Services.Interfaces.Logging;
using slick.Application.Validations;
using slick.Domain.Entities.Identity;
using slick.Domain.Interfaces.Authentication;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace slick.Application.Services.Implementations.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ITokenManagement _tokenManagement;
        private readonly IUserManagement _userManagement;
        private readonly IRoleManagement _roleManagement;
        private readonly IAppLogger<AuthenticationService> _logger;
        private readonly IRolePermissionService _rolePermissionService;
        private readonly IMapper _mapper;
        private readonly IValidator<LoginUser> _loginUserValidator;
        private readonly IValidationService _validationService;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IFileStorageService _fileStorageService;
        private readonly string _appUrl;

        public AuthenticationService(
            ITokenManagement tokenManagement,
            IUserManagement userManagement,
            IRoleManagement roleManagement,
            IAppLogger<AuthenticationService> logger,
            IRolePermissionService rolePermissionService,
            IMapper mapper,
            IValidator<LoginUser> loginUserValidator,
            IValidationService validationService,
            IBackgroundJobService backgroundJobService,
            IFileStorageService fileStorageService,
            IOptions<AppSettings> options)
        {
            _tokenManagement = tokenManagement;
            _userManagement = userManagement;
            _roleManagement = roleManagement;
            _logger = logger;
            _mapper = mapper;
             _rolePermissionService = rolePermissionService;
            _loginUserValidator = loginUserValidator;
            _validationService = validationService;
            _backgroundJobService = backgroundJobService;
            _fileStorageService = fileStorageService;
            _appUrl = options.Value.AppUrl;
        }

        public async Task<ServiceResponse> CreateUser(CreateUser user)
        {
           
            // Map to AppUser
            var mappedModel = _mapper.Map<AppUser>(user);
            mappedModel.UserName = user.UserName;
            mappedModel.PasswordHash = user.Password;
            mappedModel.ProfilePictureUrl = "";
            mappedModel.IsActive = true;

            // Create the user
            var result = await _userManagement.CreateUser(mappedModel);
            if (!result)
                return new ServiceResponse(false, null, "Email address might already be in use or an unknown error occurred");

            // Retrieve the user we just created
            var createdUser = await _userManagement.GetUserByEmail(user.Email);

            if (createdUser != null)
            {
                // Assign role from DTO
                bool roleAssigned = await _userManagement.AssignRoleToUser(createdUser.Id, user.RoleId);
                if (!roleAssigned)
                    return new ServiceResponse(false, null, "User created but failed to assign role");

                // Send verification email if applicable
                if (!string.IsNullOrEmpty(createdUser.Email))
                {
                    _backgroundJobService.Enqueue<AuthenticationService>(service =>
                        service.SendVerificationEmail(createdUser.Email));
                }
            }

            return new ServiceResponse(true, null, "Account created and role assigned!");
        }
        public async Task<LoginResponse> LoginUser(LoginUser user)
        {
            var validationResult = await _validationService.ValidateAsync(user, _loginUserValidator);
            if (!validationResult.Success)
                return new LoginResponse(false, validationResult.Message ?? "Validation failed");

            var mappedModel = _mapper.Map<AppUser>(user);
            mappedModel.PasswordHash = user.Password;

            bool loginResult = await _userManagement.LoginUser(mappedModel);
            if (!loginResult)
                return new LoginResponse(false, "Email not found or invalid credentials");

            bool checkedVerification = await _userManagement.CheckUserVerification(mappedModel);
            if (!checkedVerification)
                return new LoginResponse(false, "Email is not verified");

            var _user = await _userManagement.GetUserByEmail(user.Email);
            if (_user == null)
                return new LoginResponse(false, "User not found");

            // Get base claims (like Name, Email, etc.)
            var claims = await _userManagement.GetUserClaims(_user.Email!);

            // Get all roles of the user
            var roles = await _roleManagement.GetUserRoles(_user);

            // Prepare list for role permissions
            var rolePermissions = new List<GetRolePermissionDto>();

            foreach (var roleName in roles)
            {
                var role = await _roleManagement.GetRoleByNameAsync(roleName);
                if (role != null)
                {
                    var permissions = await _rolePermissionService.GetRolePermissionsByRoleIdAsync(role.Id.ToString());
                    if (permissions != null && permissions.Any())
                    {
                        rolePermissions.AddRange(permissions);
                    }
                }
            }

            // ✅ Add permissions as claims before generating JWT
            var distinctPermissions = rolePermissions
                .Select(p => p.PermissionTitle)
                .Distinct();

            foreach (var permission in distinctPermissions)
            {
                if (permission != null)
                {
                    claims.Add(new Claim("permission", permission));
                }
                
            }

            // Now generate token with permissions included
            string jwtToken = _tokenManagement.GenerateToken(claims);
            string refreshToken = _tokenManagement.GetRefreshToken();

            int saveTokenResult = await _tokenManagement.AddRefreshToken(_user.Id, refreshToken);
            if (saveTokenResult <= 0)
                return new LoginResponse(false, "Internal error occurred while authenticating");

            return new LoginResponse(true, null, jwtToken, refreshToken, _user, roles.ToList(), rolePermissions);
        }

        public async Task<LoginResponse> ReviveToken(string refreshToken)
        {
            bool validateTokenResult = await _tokenManagement.ValidateRefreshToken(refreshToken);
            if (!validateTokenResult)
                return new LoginResponse(false, "Invalid token");

            string? userId = await _tokenManagement.GetUserIdByRefreshToken(refreshToken);
            if (string.IsNullOrEmpty(userId))
                return new LoginResponse(false, "User ID not found");

            AppUser? user = await _userManagement.GetUserById(userId);
            if (user == null)
                return new LoginResponse(false, "User not found");

            if (string.IsNullOrEmpty(user.Email))
                return new LoginResponse(false, "User email is missing");

            var claims = await _userManagement.GetUserClaims(user.Email);
            string newJwtToken = _tokenManagement.GenerateToken(claims);
            string newRefreshToken = _tokenManagement.GetRefreshToken();

            await _tokenManagement.UpdateRefreshToken(userId, newRefreshToken);

            var roles = await _roleManagement.GetUserRoles(user);

            return new LoginResponse(true, null, newJwtToken, newRefreshToken, user, roles.ToList());
        }
        public async Task<List<string>> GetUserRoles(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            var user = await _userManagement.GetUserById(userId);
            if (user == null)
                return new List<string>();

            var roles = await _roleManagement.GetUserRoles(user);
            return roles.ToList();
        }
        public string HashPassword(string plainPassword)
        {
            var hasher = new PasswordHasher<object>();
            var dummy = new object();
            return hasher.HashPassword(dummy, plainPassword);
        }
        public async Task<ServiceResponse> ForgotPassword(string email, string newPassword)
        {
            var user = await _userManagement.GetUserByEmail(email);
            if (user == null)
                return new ServiceResponse(false, null, "Email not found");

            if (user.Email == null)
                return new ServiceResponse(false, null, "User's email address is not valid");

            var resetToken = _tokenManagement.GetRefreshToken();
            await _tokenManagement.AddRefreshToken(user.Id, resetToken);
            await _tokenManagement.ValidateRefreshToken(resetToken);

            _backgroundJobService.Enqueue<IEmailService>(service => service.SendPasswordResetEmail(user.Email, user.FirstName ?? "", newPassword));

            user.PasswordHash = HashPassword(newPassword);
            bool updated = await _userManagement.UpdateUsslickassword(user);
            if (!updated)
                return new ServiceResponse(false, null, "Failed to reset password");

            return new ServiceResponse(true, null, "Password reset successfully");
        }
        public async Task<ServiceResponse> SendVerificationEmail(string email)
        {
            var user = await _userManagement.GetUserByEmail(email);
            if (user == null)
                return new ServiceResponse(false, null, "Email not found");
            if (user.FirstName == null)
                return new ServiceResponse(false, null, "First name not found");
            if (string.IsNullOrWhiteSpace(user.Email))
                return new ServiceResponse(false, null, "User's email address is not valid");

            var token = Guid.NewGuid().ToString();
            await _tokenManagement.AddVerificationToken(user.Id, token);

            var verificationUrl = $"{_appUrl}/verify-token?token={token}";

            _backgroundJobService.Enqueue<IEmailService>(service =>
                service.SendVerificationEmail(user.Email, verificationUrl, user.FirstName));

            return new ServiceResponse(true, null, "Verification link sent to your email.");
        }
        public async Task<ServiceResponse> VerifyToken(string verificationToken)
        {
            var email = await _tokenManagement.GetEmailFromVerificationToken(verificationToken);
            if (string.IsNullOrEmpty(email))
                return new ServiceResponse(false, null, "Invalid or expired verification token");

            var user = await _userManagement.GetUserByEmail(email);
            if (user == null)
                return new ServiceResponse(false, null, "User not found");

            var isValid = await _tokenManagement.ValidateVerificationToken(verificationToken);
            if (!isValid)
            {
                await SendVerificationEmail(email);
                return new ServiceResponse(false, null, "Token expired. A new verification email has been sent.");
            }

            user.IsVerified = true;
            user.EmailConfirmed = true;
            var updated = await _userManagement.UpdateUserEmailStatus(user);
            if (!updated)
                return new ServiceResponse(false, null, "Failed to update user verification status");

            return new ServiceResponse(true, null, "Email verified successfully");
        }
        public async Task<ServiceResponse> Logout(string token)
        {
            string? userId = await _tokenManagement.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
                return new ServiceResponse(false, null, "User not found for token");

            bool removed = await _tokenManagement.RemoveRefreshTokenByUserId(userId);
            if (!removed)
                return new ServiceResponse(false, null, "Failed to logout");

            return new ServiceResponse(true, null, "Successfully logged out");
        }
        public async Task<ServiceResponse> GetAllUsers()
        {
            var users = await _userManagement.GetAllUsers();
            if (users == null || !users.Any())
                return new ServiceResponse(false, null, "No users found");

            var userDtos = _mapper.Map<IEnumerable<UserResponse>>(users);
            return new ServiceResponse(true, userDtos, "Users retrieved successfully");
        }
        public async Task<ServiceResponse> GetUserByToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return new ServiceResponse(false, null, "Token is required");

                string userId = await _tokenManagement.GetUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                    return new ServiceResponse(false, null, "Invalid token: User ID not found");

                var user = await _userManagement.GetUserById(userId);
                if (user == null)
                    return new ServiceResponse(false, null, "User not found");

                return new ServiceResponse(true, user, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by token");
                return new ServiceResponse(false, null, $"Error retrieving user: {ex.Message}");
            }
        }
        public async Task<ServiceResponse> UpdateUserByToken(string token, UpdateUser updateUser)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return new ServiceResponse(false, null, "Token is required");

                if (updateUser == null)
                    return new ServiceResponse(false, null, "Update data is required");

                if (string.IsNullOrEmpty(updateUser.FirstName) ||
                    string.IsNullOrEmpty(updateUser.LastName) ||
                    string.IsNullOrEmpty(updateUser.UserName) ||
                    string.IsNullOrEmpty(updateUser.PhoneNumber) ||
                    string.IsNullOrEmpty(updateUser.RoleId))
                {
                    return new ServiceResponse(false, null, "All required fields including RoleId must be provided");
                }

                string userId = await _tokenManagement.GetUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                    return new ServiceResponse(false, null, "Invalid token: User ID not found");

                var user = await _userManagement.GetUserById(userId);
                if (user == null)
                    return new ServiceResponse(false, null, "User not found");

                // Handle email update separately if email is being changed
                if (!string.IsNullOrEmpty(updateUser.Email) && updateUser.Email != user.Email)
                {
                    // Use the dedicated email update method
                    var emailUpdateResult = await UpdateEmail(token, updateUser.Email);
                    if (!emailUpdateResult.Success)
                        return emailUpdateResult;
                }

                // Update basic fields (excluding email which is handled above)
                user.FirstName = updateUser.FirstName;
                user.LastName = updateUser.LastName;
                user.UserName = updateUser.UserName;
                user.PhoneNumber = updateUser.PhoneNumber;
                user.IsActive = updateUser.IsActive;
                // Don't update EmailConfirmed here as it's handled in email update
                user.DateofBirth = updateUser.DateofBirth;
                user.Position = updateUser.Position;
                user.EmployementIdNumber = updateUser.EmployementIdNumber;
                user.TinNumber = updateUser.TinNumber;
                user.CompanyId = updateUser.CompanyId;
                user.ProfilePictureUrl = updateUser.ProfilePictureUrl;

                // Update user info in DB
                bool updated = await _userManagement.UpdateUser(user);
                if (!updated)
                    return new ServiceResponse(false, null, "Failed to update user");

                // Remove all existing roles for the user
                var existingRoles = await _userManagement.GetUserRoles(user.Id);
                foreach (var oldRole in existingRoles)
                {
                    await _userManagement.RemoveUserRole(user.Id, oldRole);
                }

                // Assign new role
                bool roleAssigned = await _userManagement.AssignRoleToUser(user.Id, updateUser.RoleId);
                if (!roleAssigned)
                    return new ServiceResponse(false, null, "User updated but failed to update role");

                return new ServiceResponse(true, user, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user by token");
                return new ServiceResponse(false, null, $"Error updating user: {ex.Message}");
            }
        }
        public async Task<ServiceResponse> UpdateProfilePictureByToken(string token, IFormFile file)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return new ServiceResponse(false, null, "Token is required");

                if (file == null || file.Length == 0)
                    return new ServiceResponse(false, null, "Profile picture is required");

                // Validate file type and size like the second method
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return new ServiceResponse(false, null, "Invalid file type. Only PNG, JPG, JPEG, GIF, and WEBP are allowed.");

                if (file.Length > 5 * 1024 * 1024)
                    return new ServiceResponse(false, null, "File size exceeds 5MB limit.");

                string userId = await _tokenManagement.GetUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                    return new ServiceResponse(false, null, "Invalid token: User ID not found");

                var user = await _userManagement.GetUserById(userId);
                if (user == null)
                    return new ServiceResponse(false, null, "User not found");

                // Use empty subfolder and remove "/Files/" prefix like the second method
                var relativePath = await _fileStorageService.SaveFileAsync(file, "");

                // Remove "/Files/" prefix if it exists
                if (!string.IsNullOrEmpty(relativePath) && relativePath.StartsWith("/Files/"))
                {
                    relativePath = relativePath.Replace("/Files/", "");
                }

                user.ProfilePictureUrl = relativePath;
                bool updated = await _userManagement.UpdateUser(user);

                if (!updated)
                    return new ServiceResponse(false, null, "Failed to update profile picture");

                return new ServiceResponse(true, null, "Profile picture updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile picture");
                return new ServiceResponse(false, null, $"Error: {ex.Message}");
            }
        }
        public async Task<ServiceResponse> UpdateUser(UpdateUser userDto)
        {
            try
            {
                var existing = await _userManagement.GetUserById(userDto.Id);
                if (existing is null)
                    return new ServiceResponse(false, null, "User not found.");

                // Map updates to existing user
                _mapper.Map(userDto, existing);

                // Update specific fields if provided
                if (!string.IsNullOrEmpty(userDto.UserName))
                    existing.UserName = userDto.UserName;

                if (!string.IsNullOrEmpty(userDto.ProfilePictureUrl))
                    existing.ProfilePictureUrl = userDto.ProfilePictureUrl;

                // Update timestamp
                existing.LastModifiedDate = DateTime.Now;

                // Update the user
                var result = await _userManagement.UpdateUser(existing);
                if (!result)
                    return new ServiceResponse(false, null, "Failed to update user.");

                // Update role if provided
                if (!string.IsNullOrEmpty(userDto.RoleId))
                {
                    // Get current role ID to check if update is needed
                    var currentRoleId = await _roleManagement.GetRoleIdByUserId(existing.Id);
                    if (currentRoleId == null)
                    {
                        bool roleAssigned = await _userManagement.AssignRoleToUser(existing.Id, userDto.RoleId);
                        if (!roleAssigned)
                        {
                            _logger.LogWarning($"Failed to assign role ID {userDto.RoleId} to user {existing.Id}");
                            return new ServiceResponse(false, null, "User updated but failed to assign role");
                        }
                    }

                    // Check if the new role is different from current role
                    if (currentRoleId != userDto.RoleId)
                    {
                        // Remove current role by ID if exists
                        if (!string.IsNullOrEmpty(currentRoleId))
                        {
                            await _userManagement.RemoveUserRole(existing.Id, currentRoleId);
                        }

                        // Assign new role using role ID
                        bool roleAssigned = await _userManagement.AssignRoleToUser(existing.Id, userDto.RoleId);
                        if (!roleAssigned)
                        {
                            _logger.LogWarning($"Failed to assign role ID {userDto.RoleId} to user {existing.Id}");
                            return new ServiceResponse(false, null, "User updated but failed to assign role");
                        }
                    }
                }

                return new ServiceResponse(true, null, "User updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating user");
                return new ServiceResponse(false, null, "Exception occurred while updating user.");
            }
        }
        public async Task<ServiceResponse> UpdateEmail(string token, string newEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return new ServiceResponse(false, null, "Token is required");

                if (string.IsNullOrEmpty(newEmail))
                    return new ServiceResponse(false, null, "New email is required");

                // Validate email format
                if (!IsValidEmail(newEmail))
                    return new ServiceResponse(false, null, "Invalid email format");

                string userId = await _tokenManagement.GetUserIdFromToken(token);
                if (string.IsNullOrEmpty(userId))
                    return new ServiceResponse(false, null, "Invalid token: User ID not found");

                var user = await _userManagement.GetUserById(userId);
                if (user == null)
                    return new ServiceResponse(false, null, "User not found");

                // Check if new email is already in use by another user
                var existingUser = await _userManagement.GetUserByEmail(newEmail);
                if (existingUser != null && existingUser.Id != userId)
                    return new ServiceResponse(false, null, "Email is already in use by another user");

                // Store old email for verification purposes
                var oldEmail = user.Email;

                // Update email (but don't mark as verified yet)
                user.Email = newEmail;
                user.EmailConfirmed = false;
                user.IsVerified = false;

                bool updated = await _userManagement.UpdateUser(user);
                if (!updated)
                    return new ServiceResponse(false, null, "Failed to update email");

                // Send verification email to the new email address
                await SendVerificationEmail(newEmail);

                return new ServiceResponse(true, null, "Email updated successfully. Please verify your new email address.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email");
                return new ServiceResponse(false, null, $"Error updating email: {ex.Message}");
            }
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
