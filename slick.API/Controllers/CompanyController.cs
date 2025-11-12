using slick.Application.DTOs;
using slick.Application.DTOs.Company;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] CreateCompanyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _companyService.CreateCompanyAsync(dto);
            return StatusCode(result.Success ? 201 : 400, result);
        }

        [HttpPut("update/{id}")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateCompanyDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            // Parse URL id to Guid
            if (!Guid.TryParse(id, out Guid parsedId))
                return BadRequest("Invalid ID format");

            // Assign URL ID to DTO
            dto.ID = parsedId;
            dto.ModifiedBy = userId;

            var result = await _companyService.UpdateCompanyAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _companyService.GetCompanyByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _companyService.GetAllCompanyAsync();
            return Ok(result);
        }

        [HttpGet("get-paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedCompanies([FromQuery] string? search)
        {
            try
            {
                var companys = await _companyService.GetPagedCompanysAsync(search);
                return Ok(companys);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving Company");
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
            var result = await _companyService.DeleteCompanyAsync(id, userId);
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
                var count = await _companyService.MultiSoftDeleteCompanysAsync(guids, userId);
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
                return StatusCode(500, $"Error soft deleting Company: {ex.Message}");
            }
        }

        [HttpPost("multi-hard-delete")]
        [Authorize]
        public async Task<IActionResult> MultiHardDelete([FromBody] List<string> ids)
        {
            try
            {

                var guids = ids.Select(Guid.Parse).ToList();
                var count = await _companyService.MultiHardDeleteCompanysAsync(guids);
                return Ok(new { DeletedCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting Company: {ex.Message}");
            }
        }

        [HttpGet("get-trash-compaines")]
        public async Task<IActionResult> GetTrashedCompanies([FromQuery] string? search, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _companyService.GetTrashedCompanysAsync(search);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving trashed Companies");
            }
        }

        [HttpPut("recover")]
        [Authorize]
        public async Task<IActionResult> Recover(string id)
        {
            if (!Guid.TryParse(id, out var companyId))
            {
                return BadRequest(new
                {
                    title = "Invalid Car Drive Type ID format",
                    status = 400,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            bool success = await _companyService.RecoverCompanyAsync(companyId);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("multi-recovery")]
        [Authorize]
        public async Task<IActionResult> MultiRecover([FromBody] List<string> ids)
        {
            try
            {
                var guids = ids.Select(Guid.Parse).ToList();
                var count = await _companyService.MultiRecoverCompanysAsync(guids);
                return Ok(new { RecoveredCount = count });
            }
            catch (FormatException)
            {
                return BadRequest("One or more IDs are invalid");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error restoring Company: {ex.Message}");
            }
        }

        [HttpDelete("hard-delete")]
        [Authorize]
        public async Task<IActionResult> HardDelete(string id)
        {
            if (!Guid.TryParse(id, out var carColorId))
            {
                return BadRequest(new
                {
                    title = "Invalid Company ID format",
                    status = 400,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            var result = await _companyService.HardDeleteCompanyAsync(carColorId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("deleted-count")]
        [Authorize]
        public async Task<IActionResult> GetDeletedCarDriveTypeCount()
        {
            try
            {
                var deletedCount = await _companyService.GetDeletedCompanysCountAsync();
                return Ok(new { Count = deletedCount });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the count of deleted Company");
            }
        }

        [HttpGet("get-name")]
        [Authorize]
        public async Task<IActionResult> GetCompanyNames(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _companyService.GetCompanyNamesAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while retrieving the name  of the Company");
            }
        }

    }
}