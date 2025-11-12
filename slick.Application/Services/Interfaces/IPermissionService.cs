using erp.Application.DTOs;
using erp.Application.DTOs.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<GetPermissionDto>> GetAllPermissionsAsync();
        Task<GetPermissionDto?> GetPermissionByIdAsync(Guid id);
    }
}
