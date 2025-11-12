using Microsoft.AspNetCore.Http;

namespace slick.Application.DTOs.Company
{
    public class UpdateCompanyDto : CompanyBaseDto
    {
        public Guid ID { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public IFormFile? Logo { get; set; }
        public bool RemoveLogo { get; set; } // Flag to remove existing logo
                                            
    }
}
