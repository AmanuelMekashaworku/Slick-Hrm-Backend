using slick.Application.DTOs.RolePermission;

namespace slick.Application.DTOs.Role
{
    public class RoleBaseDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool? Editable { get; set; }
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }
}
