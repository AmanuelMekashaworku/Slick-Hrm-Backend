using slick.Application.DTOs;
using slick.Application.DTOs.Branch;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateBranchDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }
            dto.CreatedBy = userId;
            var result = await _branchService.CreateBranchAsync(dto);
            return StatusCode(result.Success ? 201 : 400, result);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateBranchDto dto)
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
            dto.CreatedBy = userId;
            dto.ModifiedBy = userId;
            var result = await _branchService.UpdateBranchAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _branchService.GetBranchByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _branchService.GetAllBranchsAsync();
            return Ok(result);
        }

        [HttpGet("get-paged")]
        [Authorize(Policy = "View Branch")]
        public async Task<IActionResult> GetPagedBranchs([FromQuery] string? search)
        {
            try
            {
                var campuses = await _branchService.GetPagedBranchAsync(search);
                return Ok(campuses);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving Branchs");
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
            var result = await _branchService.DeleteBranchAsync(id, userId);
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
                var count = await _branchService.MultiSoftDeleteBranchAsync(guids, userId);
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
                return StatusCode(500, $"Error soft deleting Branches: {ex.Message}");
            }
        }

        [HttpPost("multi-hard-delete")]
        [Authorize]
        public async Task<IActionResult> MultiHardDelete([FromBody] List<string> ids)
        {
            try
            {

                var guids = ids.Select(Guid.Parse).ToList();
                var count = await _branchService.MultiHardDeleteBranchsAsync(guids);
                return Ok(new { DeletedCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting branches: {ex.Message}");
            }
        }

        [HttpGet("get-trash-branches")]
        public async Task<IActionResult> GetTrashedBranches([FromQuery] string? search, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _branchService.GetTrashedBranchAsync(search);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving trashed branches");
            }
        }

        [HttpPut("recover")]
        [Authorize]
        public async Task<IActionResult> Recover(string id)
        {
            if (!Guid.TryParse(id, out var branchId))
            {
                return BadRequest(new
                {
                    title = "Invalid branch ID format",
                    status = 400,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            bool success = await _branchService.RecoverBranchAsync(branchId);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("multi-recovery")]
        [Authorize]
        public async Task<IActionResult> MultiRecover([FromBody] List<string> ids)
        {
            try
            {
                var guids = ids.Select(Guid.Parse).ToList();
                var count = await _branchService.MultiRecoverBranchsAsync(guids);
                return Ok(new { RecoveredCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error restoring branchs: {ex.Message}");
            }
        }

        [HttpDelete("hard-delete")]
        [Authorize]
        public async Task<IActionResult> HardDelete(string id)
        {
            if (!Guid.TryParse(id, out var branchId))
            {
                return BadRequest(new
                {
                    title = "Invalid branchs ID format",
                    status = 400,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            var result = await _branchService.HardDeleteBranchsAsync(branchId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("deleted-count")]
        [Authorize]
        public async Task<IActionResult> GetDeletedBranchesCount()
        {
            try
            {
                var deletedCount = await _branchService.GetDeletedBranchsCountAsync();
                return Ok(new { Count = deletedCount });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the count of deleted branches");
            }
        }

        [HttpGet("get-name")]
        [Authorize]
        public async Task<IActionResult> GetBranchesNames(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _branchService.GetBranchNameAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while retrieving the name  of the branches");
            }
        }

        [HttpGet("by-company/{companyId}")]
        [Authorize]
        public async Task<ActionResult<List<GetBranchDto>>> GetBranchesByCompany(Guid companyId)
        {
            if (companyId == Guid.Empty)
                return BadRequest("Invalid company ID.");

            var branches = await _branchService.GetBranchesByCompanyIdAsync(companyId);

            if (branches == null || !branches.Any())
                return NotFound("No branches found for this company.");

            return Ok(branches);
        }

    }
}