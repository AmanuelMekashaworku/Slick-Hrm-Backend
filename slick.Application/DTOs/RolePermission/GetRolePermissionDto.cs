namespace slick.Application.DTOs.RolePermission
{
    public class GetRolePermissionDto : RolePermissionBaseDto
    {
        public Guid ID { get; set; }
        public  string? PermissionTitle { get; set; }
        public  string? RoleName { get; set; }
        public string? Role { get; set; }
        //public string? Permission { get; set; }
    }
}

