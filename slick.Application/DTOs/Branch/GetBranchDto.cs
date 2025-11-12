namespace slick.Application.DTOs.Branch
{
    public class GetBranchDto : BranchBaseDto
    {
        public Guid ID { get; set; }
        public string CompanyName { get; set; } = default!;

        public int DaysUntilHardDelete { get; set; }
    }
}