using slick.Application.DTOs;
using slick.Application.DTOs.Identity;
using slick.Application.DTOs.Role;
using slick.Application.DTOs.RolePermission;
using slick.Application.Services.Implementations;
using slick.Application.Services.Interfaces;
using slick.Application.Services.Interfaces.Authentication;
using slick.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SMS.Domain.Models;
using System.Security.Claims;
using System.Security.Cryptography;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;


        public AuthenticationController(IAuthenticationService authenticationService, IUserService userService)
        {
            _authenticationService = authenticationService;
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUser user)
        {
            if (user == null)
                return BadRequest(new ServiceResponse(false, null, "User data is required."));

            var result = await _authenticationService.CreateUser(user);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUser user)
        {
            if (user == null)
            {
                Log.Warning("LoginUser failed: User data is null");
                return BadRequest(new ServiceResponse(false, null, "Login data is required."));
            }

            try
            {
                Log.Information("Login attempt for email: {Email}", user.Email);
                var result = await _authenticationService.LoginUser(user);
                if (!result.Success)
                {
                    Log.Warning("Login failed for email: {Email}, Message: {Message}", user.Email, result.Message ?? "Login failed");
                    return BadRequest(new ServiceResponse(false, null, result.Message ?? "Login failed"));
                }

                Log.Information("Login succeeded for email: {Email}, UserId: {UserId}", user.Email, result.User?.Id);
                SetAuthCookies(result.Token!, result.RefreshToken!);
                return Ok(new
                {
                    success = result.Success,
                    user = new
                    {
                        id = result.User?.Id,
                        username = result.User?.UserName,
                        email = result.User?.Email,
                        firstName = result.User?.FirstName,
                        lastName = result.User?.LastName,
                        fullName = $"{result.User?.FirstName ?? ""} {result.User?.LastName ?? ""}".Trim(),
                        isVerified = result.User?.IsVerified ?? false,
                        isActive = result.User?.IsActive ?? false,
                        profilePictureUrl = result.User?.ProfilePictureUrl,
                        permissions= result.RolesWithPermissions ?? new List<GetRolePermissionDto>(),
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoginUser exception for email: {Email}", user.Email);
                return StatusCode(500, new ServiceResponse(false, null, "Internal server error during login"));
            }
        }
        [HttpPost("refreshToken")]
        public async Task<IActionResult> ReviveToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new ServiceResponse(false, null, "No refresh token provided."));

            var result = await _authenticationService.ReviveToken(refreshToken);
            if (!result.Success)
                return BadRequest(new ServiceResponse(false, null, result.Message ?? "Token refresh failed"));

            SetAuthCookies(result.Token!, result.RefreshToken!);
            return Ok(new
            {
                success = result.Success,
                user = new
                {
                    id = result.User?.Id,
                    username = result.User?.UserName,
                    email = result.User?.Email,
                    firstName = result.User?.FirstName,
                    lastName = result.User?.LastName,
                    fullName = $"{result.User?.FirstName ?? ""} {result.User?.LastName ?? ""}".Trim(),
                    isVerified = result.User?.IsVerified ?? false,
                    isActive = result.User?.IsActive ?? false,
                    profilePictureUrl = result.User?.ProfilePictureUrl,
                    roles = result.Roles ?? new List<string>()
                }
            });
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Cookies["jwt"];
            Log.Information("Logout attempt for token: {Token}", token ?? "No token provided");

            try
            {
                var result = await _authenticationService.Logout(token ?? string.Empty);
                if (!result.Success)
                {
                    Log.Warning("Logout failed: {Message}", result.Message ?? "Logout failed");
                    return BadRequest(result);
                }

                ClearAuthCookies();
                Log.Information("Logout succeeded, cookies cleared");
                return Ok(new ServiceResponse(true, null, "Logged out successfully"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Logout exception for token: {Token}", token);
                return StatusCode(500, new ServiceResponse(false, null, "Internal server error during logout"));
            }
        }
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new ServiceResponse(false, null, "Email is required."));

            var newPassword = GenerateRandomPassword();
            var result = await _authenticationService.ForgotPassword(email, newPassword);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpPost("verifyToken")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyEmail dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.VerificationToken))
                return BadRequest(new ServiceResponse(false, null, "Verification token is required."));

            var result = await _authenticationService.VerifyToken(dto.VerificationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken()
        {
            var token = Request.Cookies["jwt"];
            Log.Information("ValidateToken called. Token: {Token}", token ?? "No token provided");

            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("ValidateToken failed: No token provided in cookies");
                return Unauthorized(new ServiceResponse(false, null, "No token provided."));
            }

            try
            {
                var result = await _authenticationService.GetUserByToken(token);
                if (!result.Success || result.Data == null)
                {
                    Log.Warning("ValidateToken failed: {Message}", result.Message ?? "Token validation failed");
                    return Unauthorized(new ServiceResponse(false, null, result.Message ?? "Token validation failed"));
                }

                var user = result.Data as AppUser;
                var roles = await _authenticationService.GetUserRoles(user?.Id);
                Log.Information("ValidateToken succeeded for user: {UserId}, Username: {Username}", user?.Id, user?.UserName);

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = user?.Id,
                        username = user?.UserName,
                        email = user?.Email,
                        firstName = user?.FirstName,
                        lastName = user?.LastName,
                        fullName = $"{user?.FirstName ?? ""} {user?.LastName ?? ""}".Trim(),
                        isVerified = user?.IsVerified ?? false,
                        isActive = user?.IsActive ?? false,
                        profilePictureUrl = user?.ProfilePictureUrl,
                        roles = roles ?? new List<string>()
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ValidateToken exception: Token: {Token}", token);
                return StatusCode(500, new ServiceResponse(false, null, "Internal server error during token validation"));
            }
        }
        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _authenticationService.GetAllUsers();
            return Ok(result);
        }
        [HttpGet("get-paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedUsers([FromQuery] string? search)
        {
            try
            {
                var users = await _userService.GetPagedUserAsync(search);
                return Ok(users);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving Users");
            }
        }
        [HttpGet("getByToken")]
        [Authorize]
        public async Task<IActionResult> GetByToken()
        {
            var token = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
                return BadRequest(new ServiceResponse(false, null, "No token provided."));

            var result = await _authenticationService.GetUserByToken(token);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpPut("updateByToken")]
        [Authorize]
        public async Task<IActionResult> UpdateByToken([FromBody] UpdateUser updateUser)
        {
            if (updateUser == null)
                return BadRequest(new ServiceResponse(false, null, "Update data is required."));

            var token = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
                return BadRequest(new ServiceResponse(false, null, "No token provided."));

            var result = await _authenticationService.UpdateUserByToken(token, updateUser);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpPost("uploadProfilePicture")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ServiceResponse(false, null, "Invalid file."));

            var token = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
                return BadRequest(new ServiceResponse(false, null, "No token provided."));

            var result = await _authenticationService.UpdateProfilePictureByToken(token, file);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("check-auth")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            return Ok(new ServiceResponse(true, new { authenticated = true }, "Authenticated"));
        }
        private void SetAuthCookies(string token, string refreshToken)
        {
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(1),
                Path = "/"
            });

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
        }
        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("jwt", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }
        private string GenerateRandomPassword(int length = 12)
        {
            if (length < 4) throw new ArgumentException("Password length must be at least 4 characters.");

            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@$?_-";
            const string allValid = letters + digits + special;

            var passwordChars = new char[length];
            passwordChars[0] = letters[RandomNumberGenerator.GetInt32(letters.Length)];
            passwordChars[1] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            passwordChars[2] = special[RandomNumberGenerator.GetInt32(special.Length)];

            for (int i = 3; i < length; i++)
            {
                passwordChars[i] = allValid[RandomNumberGenerator.GetInt32(allValid.Length)];
            }

            for (int i = length - 1; i >= 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
            }

            return new string(passwordChars);
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var result = await _userService.DeleteUserAsync(id, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("multi-soft-delete")]
        [Authorize]
        public async Task<IActionResult> MultiSoftDelete([FromBody] List<string> ids)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token");

                var count = await _userService.MultiSoftDeleteUserAsync(ids, userId);
                return Ok(new { DeletedCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error soft deleting user: {ex.Message}");
            }
        }

        [HttpPost("multi-hard-delete")]
        [Authorize]
        public async Task<IActionResult> MultiHardDelete([FromBody] List<string> ids)
        {
            try
            {
                var count = await _userService.MultiHardDeleteUserAsync(ids);
                return Ok(new { DeletedCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting user: {ex.Message}");
            }
        }

        [HttpGet("get-trash-user")]
        [Authorize]
        public async Task<IActionResult> GetTrashed([FromQuery] string? search)
        {
            try
            {
                var result = await _userService.GetTrashedUserAsync(search);
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "An error occurred while retrieving trashed user.");
            }
        }

        [HttpPut("recover")]
        [Authorize]
        public async Task<IActionResult> Recover(string id)
        {
         

            bool success = await _userService.RecoverUserAsync(id);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("multi-recovery")]
        [Authorize]
        public async Task<IActionResult> MultiRecover([FromBody] List<string> ids)
        {
            try
            {
                var count = await _userService.MultiRecoverUserAsync(ids);
                return Ok(new { RecoveredCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error restoring user: {ex.Message}");
            }
        }

        [HttpDelete("hard-delete")]
        [Authorize]
        public async Task<IActionResult> HardDelete(string id)
        {
       
            var result = await _userService.HardDeleteUserAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("deleted-count")]
        [Authorize]
        public async Task<IActionResult> GetDeletedCount()
        {
            try
            {
                var count = await _userService.GetDeletedUserCountAsync();
                return Ok(new { Count = count });
            }
            catch
            {
                return StatusCode(500, "An error occurred while retrieving the count of deleted user.");
            }
        }

        [HttpGet("get-name")]
        [Authorize]
        public async Task<IActionResult> GetNames(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userService.GetUserNamesAsync(cancellationToken);
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "An error occurred while retrieving user name.");
            }
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateUser dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set modified by
            dto.ModifiedBy = userId;

            var result = await _authenticationService.UpdateUser(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}