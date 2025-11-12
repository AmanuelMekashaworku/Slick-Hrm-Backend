using Microsoft.AspNetCore.Http;

namespace slick.Application.DTOs.Company
{
    public class CreateCompanyDto : CompanyBaseDto
    {
        public IFormFile? Logo { get; set; } // Add this
    }
}
