using slick.Application.DTOs;
using slick.Application.DTOs.ControllerAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slick.Application.Services.Interfaces
{
    public interface IControllerActionService
    {
     
        Task<GetControllerActionDto?> GetControllerActionByIdAsync(Guid id);
        Task<List<GetControllerActionDto>> GetPagedControllerActionAsync(string? search);
    }
}
