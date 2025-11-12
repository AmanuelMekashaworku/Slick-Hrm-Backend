using System.ComponentModel.DataAnnotations;

namespace slick.Domain.Models
{
   public  class ControllerAction
    {

        [Key]
        public Guid Id { get; set; }
        public Guid TaskControllerId { get; set; }
        public required TaskController TaskController { get; set; }
        public Guid ActionTaskId { get; set; }
        public virtual required slick.Domain.Entities.ActionTask ActionTask { get; set; }
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
