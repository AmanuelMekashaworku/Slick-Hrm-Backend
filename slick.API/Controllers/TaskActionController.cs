using slick.Application.DTOs.TaskAction;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskActionController : ControllerBase
    {
        private readonly ITaskActionService _taskActionService;

        public TaskActionController(ITaskActionService taskActionService)
        {
            _taskActionService = taskActionService;
        }
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _taskActionService.GetTaskActionByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _taskActionService.GetAllTaskActionsAsync();
            return Ok(result);
        }

       

    }
}