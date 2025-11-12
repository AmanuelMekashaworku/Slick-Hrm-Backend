
using slick.Application.DTOs.RolePermission;

namespace slick.Application.DTOs.Role
{
    public class GetRoleDto : RoleBaseDto
    {
        public String? ID { get; set; }
        public int DaysUntilHardDelete { get; set; }
        public List<GetRolePermissionDto> Permissions { get; set; } = new List<GetRolePermissionDto>();

    }
}
