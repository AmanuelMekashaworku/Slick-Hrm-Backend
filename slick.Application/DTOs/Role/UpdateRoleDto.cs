namespace slick.Application.DTOs.Role
{
    public class UpdateRoleDto : RoleBaseDto
    {
        public required String ID { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
