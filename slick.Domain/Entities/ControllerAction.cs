using slick.Domain.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMS.Domain.Models
{
   public  class ControllerAction
    {

        [Key]
        public Guid Id { get; set; }
        public Guid TaskControllerId { get; set; }
        public required TaskController TaskController { get; set; }
        public Guid ActionTaskId { get; set; }
        public required ActionTask ActionTask { get; set; }
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
