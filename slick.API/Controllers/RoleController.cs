using slick.Application.DTOs;
using slick.Application.DTOs.Role;
using slick.Application.DTOs.RolePermission;
using slick.Application.DTOs.TaskController;
using slick.Application.Services.Implementations;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.CreateRoleAsync(dto);
            return StatusCode(result.Success ? 201 : 400, result);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            if (dto.ID != dto.ID)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            dto.ModifiedBy = userId;
            var result = await _roleService.UpdateRoleAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _roleService.GetRoleByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _roleService.GetAllRolesAsync();
            return Ok(result);
        }

        [HttpGet("get-paged")]
        //[Authorize]
        public async Task<IActionResult> GetPagedRoles([FromQuery] string? search)
        {
            try
            {
                var campuses = await _roleService.GetPagedRoleAsync(search);
                return Ok(campuses);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving Role");
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(String id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            var result = await _roleService.DeleteRoleAsync(id, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("multi-soft-delete")]
        [Authorize]
        public async Task<IActionResult> MultiSoftDelete([FromBody] List<string> ids)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Skip Guid parsing and pass string IDs directly
                var count = await _roleService.MultiSoftDeleteRoleAsync(ids, userId);
                return Ok(new { DeletedCount = count });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error soft deleting Role: {ex.Message}");
            }
        }

        [HttpPost("multi-hard-delete")]
        [Authorize]
        public async Task<IActionResult> MultiHardDelete([FromBody] List<string> ids)
        {
            try
            {

                var count = await _roleService.MultiHardDeleteRolesAsync(ids);
                return Ok(new { DeletedCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting Role: {ex.Message}");
            }
        }

        [HttpGet("get-trash-role")]
        public async Task<IActionResult> GetTrashedRoles([FromQuery] string? search, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.GetTrashedRoleAsync(search);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving trashed Role");
            }
        }
        [HttpPut("recover")]
        [Authorize]
        public async Task<IActionResult> Recover(string id)
        {
          
            bool success = await _roleService.RecoverRoleAsync(id);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("multi-recovery")]
        [Authorize]
        public async Task<IActionResult> MultiRecover([FromBody] List<string> ids)
        {
            try
            {
              
                var count = await _roleService.MultiRecoverRolesAsync(ids);
                return Ok(new { RecoveredCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error restoring Role: {ex.Message}");
            }
        }

        [HttpDelete("hard-delete")]
        [Authorize]
        public async Task<IActionResult> HardDelete(string id)
        {
           
            var result = await _roleService.HardDeleteRoleAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("deleted-count")]
        [Authorize]
        public async Task<IActionResult> GetDeletedRoleCount()
        {
            try
            {
                var deletedCount = await _roleService.GetDeletedRolesCountAsync();
                return Ok(new { Count = deletedCount });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the count of deleted Role");
            }
        }

        [HttpGet("get-name")]
        [Authorize]
        public async Task<IActionResult> GetRoleNames(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _roleService.GetRoleNamesAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while retrieving the name  of the Role ");
            }
        }

        [HttpPost("add-role-permissions")]
        //[Authorize]
        public async Task<IActionResult> AddRolePermissions([FromBody] AddRolePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.AddRolePermissionAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("remove-role-permission")]
        public async Task<ActionResult<ServiceResponse>> RemoveRolePermission([FromQuery] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            var result = await _roleService.DeleteRolePermissionAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }


    }
}