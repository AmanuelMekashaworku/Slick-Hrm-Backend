using slick.Application.DTOs;
using slick.Application.DTOs.TaskController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slick.Application.Services.Interfaces
{
    public interface ITaskControllerService
    {
      
        Task<List<GetTaskControllerDto>> GetAllTaskControllersAsync();
        Task<GetTaskControllerDto?> GetTaskControllerByIdAsync(Guid id);
       
    }
}
