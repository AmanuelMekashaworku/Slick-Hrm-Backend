namespace slick.Application.DTOs.Company
{
    public class GetCompanyDto : CompanyBaseDto
    {
        public Guid ID { get; set; }
        public string BusinessGroupName { get; set; } = default!;
        public List<string> BranchNames { get; set; } = new();
        public string? Logo { get; set; }
        public int DaysUntilHardDelete { get; set; }
    }
}
