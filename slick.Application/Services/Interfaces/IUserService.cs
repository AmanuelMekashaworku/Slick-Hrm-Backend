using erp.Application.DTOs;
using erp.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse> DeleteUserAsync(string id, string userId);
        Task<int> GetDeletedUserCountAsync();
        Task<List<GetUser>> GetPagedUserAsync(string? search);
        Task<List<GetUser>> GetTrashedUserAsync(string? search);
        //Task<GetUser?> GetUserByIdAsync(Guid id);
        Task<GetUser?> GetUserByIdAsync(string id);
        Task<IEnumerable<AppUser>> GetUserNamesAsync(CancellationToken cancellationToken = default);
        Task<ServiceResponse> HardDeleteUserAsync(string id);
        Task<int> MultiHardDeleteUserAsync(List<string> ids, CancellationToken cancellationToken = default);
        Task<int> MultiRecoverUserAsync(List<string> ids, CancellationToken cancellationToken = default);
        Task<int> MultiSoftDeleteUserAsync(List<string> ids, string userId, CancellationToken cancellationToken = default);
        Task<bool> RecoverUserAsync(string id, CancellationToken cancellationToken = default);
    }
}
