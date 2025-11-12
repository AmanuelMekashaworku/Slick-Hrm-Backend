using erp.Application.DTOs;
using erp.Application.DTOs.TaskController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface ITaskControllerService
    {
      
        Task<List<GetTaskControllerDto>> GetAllTaskControllersAsync();
        Task<GetTaskControllerDto?> GetTaskControllerByIdAsync(Guid id);
       
    }
}
