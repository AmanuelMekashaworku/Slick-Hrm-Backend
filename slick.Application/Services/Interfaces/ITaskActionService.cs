using erp.Application.DTOs;
using erp.Application.DTOs.TaskAction;

namespace erp.Application.Services.Interfaces
{
    public interface ITaskActionService
    {
        Task<List<GetTaskActionDto>> GetAllTaskActionsAsync();
        Task<GetTaskActionDto?> GetTaskActionByIdAsync(Guid id);
    }
}
