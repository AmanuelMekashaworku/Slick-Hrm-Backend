using erp.Application.DTOs;
using erp.Application.DTOs.Branch;

namespace erp.Application.Services.Interfaces
{
    public interface IBranchService
    {
        Task<ServiceResponse> CreateBranchAsync(CreateBranchDto branchDto);
        Task<ServiceResponse> DeleteBranchAsync(Guid id, string userid);
        Task<List<GetBranchDto>> GetAllBranchsAsync();
        Task<GetBranchDto?> GetBranchByIdAsync(Guid id);
        Task<List<GetBranchDto>> GetBranchesByCompanyIdAsync(Guid companyId);
        Task<IEnumerable<GetBranchNameDto>> GetBranchNameAsync(CancellationToken cancellationToken = default);
        Task<int> GetDeletedBranchsCountAsync();
        Task<List<GetBranchDto>> GetPagedBranchAsync(string? search);
        Task<List<GetBranchDto>> GetTrashedBranchAsync(string? search);
        Task<ServiceResponse> HardDeleteBranchsAsync(Guid id);
        Task<int> MultiHardDeleteBranchsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiRecoverBranchsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiSoftDeleteBranchAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default);
        Task<bool> RecoverBranchAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ServiceResponse> UpdateBranchAsync(UpdateBranchDto branchDto);
    }
}
