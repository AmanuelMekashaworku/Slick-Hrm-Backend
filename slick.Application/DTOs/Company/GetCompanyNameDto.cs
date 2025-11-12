namespace slick.Application.DTOs.Company
{
    public class GetCompanyNameDto
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = default!;
        public string Website { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? MobileNo { get; set; }
        public string? OfficePhone { get; set; }
        public string TinNo { get; set; } = default!;
        public string? Logo { get; set; }
    }

}