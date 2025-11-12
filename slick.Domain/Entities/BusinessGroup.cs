using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slick.Domain.Entities
{
    public class BusinessGroup
    {
        [Key]
        public Guid ID { get; set; }
        public required string BusinessGroupName { get; set; }
        public required string BusinessGroupDescription { get; set; }
        public ICollection<Company> Companies { get; set; } = new List<Company>();
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
