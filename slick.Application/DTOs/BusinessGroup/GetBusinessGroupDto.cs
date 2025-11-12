namespace slick.Application.DTOs.BusinessGroup
{
    public class GetBusinessGroupDto : BusinessGroupBaseDto
    {
        public Guid Id { get; set; }
        public List<string>? CompanyNames { get; set; }
        public int DaysUntilHardDelete { get; set; }
    }
}
