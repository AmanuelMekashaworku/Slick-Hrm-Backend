namespace slick.Application.DTOs.RolePermission
{
    public class AddRolePermissionDto
    {
        public String? RoleId { get; set; }
        public List<Guid> PermissionIds { get; set; } = new();
    }
}