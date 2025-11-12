using slick.Application.DTOs.Permission;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _permissionService.GetPermissionByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        //[Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _permissionService.GetAllPermissionsAsync();
            return Ok(result);
        }
    }
}