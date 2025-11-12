using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.TaskAction;
using slick.Application.Services.Interfaces;
using slick.Domain.Models;
using slick.Domain.Interfaces;
using System.Linq.Expressions;
using slick.Domain.Entities;

namespace slick.Application.Services.Implementations
{
    public class TaskActionService(IGeneric<ActionTask> taskActionRepository, IMapper mapper) : ITaskActionService
    {
      
        public async Task<GetTaskActionDto?> GetTaskActionByIdAsync(Guid id)
        {
            try
            {
                var data = await taskActionRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetTaskActionDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetTaskActionDto>> GetAllTaskActionsAsync()
        {
            try
            {
                var taskActions = await taskActionRepository.GetAllAsync();
                return mapper.Map<List<GetTaskActionDto>>(taskActions);
            }
            catch
            {
                return new List<GetTaskActionDto>();
            }
        }
      


    }
}
