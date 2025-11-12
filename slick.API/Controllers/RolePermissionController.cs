using slick.Application.DTOs.RolePermission;
using slick.Application.Services.Implementations;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermisionController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;

        public RolePermisionController(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateRolePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _rolePermissionService.CreateRolePermissionAsync(dto);
            return StatusCode(result.Success ? 201 : 400, result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _rolePermissionService.GetRolePermissionByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _rolePermissionService.GetAllRolePermissionsAsync();
            return Ok(result);
        }

        [HttpGet("get-paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedPermissions([FromQuery] string? search)
        {
            try
            {
                var campuses = await _rolePermissionService.GetPagedRolePermissionsAsync(search);
                return Ok(campuses);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving Permissions");
            }
        }
        [HttpGet("deleted-count")]
        [Authorize]
        public async Task<IActionResult> GetDeletedRoleCount()
        {
            try
            {
                var deletedCount = await _rolePermissionService.GetDeletedRolePermissionsCountAsync();
                return Ok(new { Count = deletedCount });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the count of deleted Role");
            }
        }

    }
}