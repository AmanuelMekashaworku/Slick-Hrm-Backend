using slick.Application.DTOs;
using slick.Application.DTOs.TaskAction;

namespace slick.Application.Services.Interfaces
{
    public interface ITaskActionService
    {
        Task<List<GetTaskActionDto>> GetAllTaskActionsAsync();
        Task<GetTaskActionDto?> GetTaskActionByIdAsync(Guid id);
    }
}
