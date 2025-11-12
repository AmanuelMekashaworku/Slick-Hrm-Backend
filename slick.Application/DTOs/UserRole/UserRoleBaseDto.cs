using slick.Application.DTOs.RolePermission;

namespace slick.Application.DTOs.UserRole
{
    public class UserRoleBaseDto
    {
        public int RoleId { get; set; }
        public Guid AppUserId { get; set; }
        public List<RolePermissionBaseDto>? RolePermissions { get; set; }
    }
}
