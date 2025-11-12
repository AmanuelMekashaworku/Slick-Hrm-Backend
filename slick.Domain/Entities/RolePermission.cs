using System;

namespace SMS.Domain.Models
{
    public class RolePermission
    {
        public Guid Id { get; set; }
        public string RoleId { get; set; } = default!;
        public virtual ApplicationRole? Role { get; set; }
        public Guid PermissionId { get; set; }
        public virtual Permission? Permission { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int DaysUntilHardDelete =>
           IsDeleted ? (45 - (DateTime.Now - DeletedDate.GetValueOrDefault()).Days) : 0;
    }
}
