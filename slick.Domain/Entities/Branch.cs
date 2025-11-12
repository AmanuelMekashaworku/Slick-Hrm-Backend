using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slick.Domain.Entities
{
    public class Branch
    {
        [Key]
        public Guid ID { get; set; }
        public required string BranchName { get; set; }
        public required string BranchAddress { get; set; }
        public Guid CompanyId { get; set; }  // foreign key
        public required Company Company { get; set; }  // navigation property
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
