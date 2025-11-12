using slick.Application.DTOs;
using slick.Application.DTOs.BusinessGroup;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessGroupController : ControllerBase
    {
        private readonly IBusinessGroupService _businessGroupService;

        public BusinessGroupController(IBusinessGroupService businessGroupService)
        {
            _businessGroupService = businessGroupService;
        }
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateBusinessGroupDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _businessGroupService.CreateBusinessGroupAsync(dto);
            return StatusCode(result.Success ? 201 : 400, result);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateBusinessGroupDto dto)
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
            var result = await _businessGroupService.UpdateBusinessGroupAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _businessGroupService.GetBusinessGroupByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _businessGroupService.GetAllBusinesGroupsAsync();
            return Ok(result);
        }

        [HttpGet("get-paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedBusinessGroups([FromQuery] string? search)
        {
            try
            {
                var campuses = await _businessGroupService.GetPagedBusinessGroupAsync(search);
                return Ok(campuses);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving Business Group");
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            var result = await _businessGroupService.DeleteBusinessGroupAsync(id, userId);
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

                var guids = ids.Select(Guid.Parse).ToList();
                var count = await _businessGroupService.MultiSoftDeleteBusinessGroupAsync(guids, userId);
                return Ok(new { DeletedCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
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
                return StatusCode(500, $"Error soft deleting Business Groups: {ex.Message}");
            }
        }

        [HttpPost("multi-hard-delete")]
        [Authorize]
        public async Task<IActionResult> MultiHardDelete([FromBody] List<string> ids)
        {
            try
            {

                var guids = ids.Select(Guid.Parse).ToList();
                var count = await _businessGroupService.MultiHardDeleteBusinessGroupsAsync(guids);
                return Ok(new { DeletedCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting Business Groups: {ex.Message}");
            }
        }

        [HttpGet("get-trash-businessGroups")]
        public async Task<IActionResult> GetTrashedBusinessGroups([FromQuery] string? search, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _businessGroupService.GetTrashedBusinessGroupAsync(search);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving trashed business Groups");
            }
        }

        [HttpPut("recover")]
        [Authorize]
        public async Task<IActionResult> Recover(string id)
        {
            if (!Guid.TryParse(id, out var businessGroupId))
            {
                return BadRequest(new
                {
                    title = "Invalid Business Group ID format",
                    status = 400,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            bool success = await _businessGroupService.RecoverBusinessGroupAsync(businessGroupId);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("multi-recovery")]
        [Authorize]
        public async Task<IActionResult> MultiRecover([FromBody] List<string> ids)
        {
            try
            {
                var guids = ids.Select(Guid.Parse).ToList();
                var count = await _businessGroupService.MultiRecoverBusinessGroupsAsync(guids);
                return Ok(new { RecoveredCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error restoring business Group: {ex.Message}");
            }
        }

        [HttpDelete("hard-delete")]
        [Authorize]
        public async Task<IActionResult> HardDelete(string id)
        {
            if (!Guid.TryParse(id, out var businessGroupId))
            {
                return BadRequest(new
                {
                    title = "Invalid business Group ID format",
                    status = 400,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            var result = await _businessGroupService.HardDeleteBusinessGroupsAsync(businessGroupId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("deleted-count")]
        [Authorize]
        public async Task<IActionResult> GetDeletedBusinessGroupsCount()
        {
            try
            {
                var deletedCount = await _businessGroupService.GetDeletedBusinessGroupsCountAsync();
                return Ok(new { Count = deletedCount });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the count of deleted Business Group ");
            }
        }

        [HttpGet("get-name")]
        [Authorize]
        public async Task<IActionResult> GetBusinessGroupNames(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _businessGroupService.GetBusinessGroupNameAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while retrieving the name  of the Business Groups");
            }
        }

    }
}