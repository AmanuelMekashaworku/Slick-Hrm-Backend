using erp.Application.DTOs;
using erp.Application.DTOs.Company;

namespace erp.Application.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<ServiceResponse> CreateCompanyAsync(CreateCompanyDto companyDto);
        Task<ServiceResponse> DeleteCompanyAsync(Guid id, string userid);
        Task<List<GetCompanyDto>> GetAllCompanyAsync();
        Task<IEnumerable<GetCompanyNameDto>> GetCompanyNamesAsync(CancellationToken cancellationToken = default);
        Task<GetCompanyDto?> GetCompanyByIdAsync(Guid id);
        Task<int> GetDeletedCompanysCountAsync();
        Task<List<GetCompanyDto>> GetPagedCompanysAsync(string? search);
        Task<List<GetCompanyDto>> GetTrashedCompanysAsync(string? search);
        Task<ServiceResponse> HardDeleteCompanyAsync(Guid id);
        Task<int> MultiHardDeleteCompanysAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiRecoverCompanysAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiSoftDeleteCompanysAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default);
        Task<bool> RecoverCompanyAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ServiceResponse> UpdateCompanyAsync(UpdateCompanyDto companyDto);
    }
}
