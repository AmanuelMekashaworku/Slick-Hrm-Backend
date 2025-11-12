namespace slick.Application.DTOs.Branch
{
    public class BranchBaseDto
    {
        public string BranchName { get; set; } = string.Empty;
        public string BranchAddress { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
