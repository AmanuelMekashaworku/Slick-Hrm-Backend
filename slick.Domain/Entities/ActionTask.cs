using System.ComponentModel.DataAnnotations;

namespace slick.Domain.Entities
{
    public class ActionTask
    {
        [Key]
        public Guid Id { get; set; }
        public required string ActionName { get; set; }
        public string? ActionDesciption { get; set; }
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
