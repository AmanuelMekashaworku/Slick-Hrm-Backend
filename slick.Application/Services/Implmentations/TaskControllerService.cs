using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.TaskController;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using slick.Domain.Models;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class TaskControllerService(IGeneric<TaskController> taskControllerRepository, IMapper mapper) : ITaskControllerService
    {
      

        public async Task<GetTaskControllerDto?> GetTaskControllerByIdAsync(Guid id)
        {
            try
            {
                var data = await taskControllerRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetTaskControllerDto>(data);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<GetTaskControllerDto>> GetAllTaskControllersAsync()
        {
            try
            {
                var suppliers = await taskControllerRepository.GetAllAsync();
                return mapper.Map<List<GetTaskControllerDto>>(suppliers);
            }
            catch
            {
                return new List<GetTaskControllerDto>();
            }
        }

      
    }
}
