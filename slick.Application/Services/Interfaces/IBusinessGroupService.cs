using slick.Application.DTOs;
using slick.Application.DTOs.BusinessGroup;
using System.Threading.Tasks;

namespace slick.Application.Services.Interfaces
{
    public interface IBusinessGroupService
    {
        Task<ServiceResponse> CreateBusinessGroupAsync(CreateBusinessGroupDto businessGroupDto);
        Task<ServiceResponse> DeleteBusinessGroupAsync(Guid id, string userid);
        Task<List<GetBusinessGroupDto>> GetAllBusinesGroupsAsync();
        Task<GetBusinessGroupDto?> GetBusinessGroupByIdAsync(Guid id);
        Task<IEnumerable<GetBusinessGroupNameDto>> GetBusinessGroupNameAsync(CancellationToken cancellationToken = default);
        Task<int> GetDeletedBusinessGroupsCountAsync();
        Task<List<GetBusinessGroupDto>> GetPagedBusinessGroupAsync(string? search);
        Task<List<GetBusinessGroupDto>> GetTrashedBusinessGroupAsync(string? search);
        Task<ServiceResponse> HardDeleteBusinessGroupsAsync(Guid id);
        Task<int> MultiHardDeleteBusinessGroupsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiRecoverBusinessGroupsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiSoftDeleteBusinessGroupAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default);
        Task<bool> RecoverBusinessGroupAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ServiceResponse> UpdateBusinessGroupAsync(UpdateBusinessGroupDto businessGroupDto);
    }
}
