using Microsoft.AspNetCore.Identity;

namespace slick.Domain.Models
{
    public class UserRole : IdentityUserRole<string>
    {

        public virtual List<RolePermission>? RolePermissions { get; set; }
        public virtual AppUser? User { get; set; }
        public virtual ApplicationRole? Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}