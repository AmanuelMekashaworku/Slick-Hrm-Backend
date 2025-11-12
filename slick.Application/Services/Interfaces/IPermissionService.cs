using slick.Application.DTOs;
using slick.Application.DTOs.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slick.Application.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<GetPermissionDto>> GetAllPermissionsAsync();
        Task<GetPermissionDto?> GetPermissionByIdAsync(Guid id);
    }
}
