using slick.Application.DTOs;
using slick.Application.DTOs.Role;
using slick.Application.DTOs.RolePermission;
using slick.Domain.Entities.Identity;
using System.Collections.Generic;

namespace slick.Application.DTOs.Identity
{
    public record LoginResponse(
        bool Success,
        string? Message,
        string? Token = null,
        string? RefreshToken = null,
        AppUser? User = null,
        List<string>? Roles = null,
        List<GetRolePermissionDto>? RolesWithPermissions = null)
        : ServiceResponse(Success, null, Message)
    {
        public LoginResponse() : this(false, null)
        {
        }

        public LoginResponse(bool success, string? message) : this(success, message, null, null, null, null, null)
        {
        }

        public LoginResponse(
            bool success,
            string token,
            string refreshToken,
            AppUser? user,
            List<string>? roles,
            List<GetRolePermissionDto>? rolesWithPermissions)
            : this(success, null, token, refreshToken, user, roles ?? new List<string>(),
                  rolesWithPermissions ?? new List<GetRolePermissionDto>())
        {
        }
    }
}