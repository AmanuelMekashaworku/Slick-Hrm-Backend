namespace slick.Domain.Models
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
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int DaysUntilHardDelete =>
           IsDeleted ? (45 - (DateTime.Now - DeletedDate.GetValueOrDefault()).Days) : 0;
    }
}
