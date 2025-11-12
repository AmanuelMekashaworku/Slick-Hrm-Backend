namespace slick.Application.DTOs.Branch
{
    public class GetBranchNameDto
    {
        public Guid ID { get; set; }
        public string BranchName { get; set; } = string.Empty;
    }
}