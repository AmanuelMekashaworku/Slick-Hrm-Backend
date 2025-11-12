using slick.Application.DTOs.Permission;
using slick.Application.Services.Implementations;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerActionController : ControllerBase
    {
        private readonly IControllerActionService _controllerActionService;

        public ControllerActionController(IControllerActionService controllerActionService)
        {
            _controllerActionService = controllerActionService;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _controllerActionService.GetControllerActionByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }
        [HttpGet("get-paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedControllerActions([FromQuery] string? search)
        {
            try
            {
                var campuses = await _controllerActionService.GetPagedControllerActionAsync(search);
                return Ok(campuses);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving Controller Action");
            }
        }

    }
}