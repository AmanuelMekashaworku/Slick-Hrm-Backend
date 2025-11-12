using System.ComponentModel.DataAnnotations;
namespace slick.Domain.Entities
{
    public class Company
    {
        [Key]
        public Guid ID { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string Website { get; set; }
        public required string Email { get; set; }
        public string? MobileNo { get; set; }
        public string? OfficePhone { get; set; }
        public required string TinNo { get; set; }
        public Guid BusinessGroupId { get; set; }
        public required BusinessGroup BusinessGroup { get; set; }
        public required ICollection<Branch> Branches { get; set; }
        public string? Logo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        // Changed from int to Guid
        public Guid CreatedBy { get; set; }          // Not nullable (required)
        public Guid? ModifiedBy { get; set; }        // Nullable
        public Guid? DeletedBy { get; set; }         // Nullable
        public DateTime? DeletedDate { get; set; } // Track when soft-delete occurred// Nullable
        public int DaysUntilHardDelete =>
           IsDeleted ? (45 - (DateTime.Now - DeletedDate.GetValueOrDefault()).Days) : 0;
    }
}