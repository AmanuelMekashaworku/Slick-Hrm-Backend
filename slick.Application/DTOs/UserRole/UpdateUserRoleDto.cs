namespace slick.Application.DTOs.UserRole
{
    public class UpdateUserRoleDto : UserRoleBaseDto
    {
        public Guid ID { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
