using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SMS.Domain.Models
{
    public class TaskController
    {

        [Key]
        public Guid Id { get; set; }
        public required string DisplayController { get; set; }
        public required string RoutingController { get; set; }
        public required string ControllerDesciption { get; set; }
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