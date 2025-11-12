namespace slick.Application.DTOs.Branch
{
    public class UpdateBranchDto : BranchBaseDto
    {
        public Guid ID { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}