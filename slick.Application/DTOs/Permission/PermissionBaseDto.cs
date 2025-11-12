namespace slick.Application.DTOs.Permission
{
    public class PermissionBaseDto
    {
        public required string PermissionTitle { get; set; }
        public Guid ControllersActionId { get; set; }
    }
}
