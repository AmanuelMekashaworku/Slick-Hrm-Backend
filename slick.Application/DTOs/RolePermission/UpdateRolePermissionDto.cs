namespace slick.Application.DTOs.RolePermission
{
    public class UpdateRolePermissionDto : RolePermissionBaseDto
    {
        public Guid ID { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
