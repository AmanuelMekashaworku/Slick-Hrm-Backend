using slick.Application.DTOs;
using slick.Application.DTOs.TaskController;
using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace slick.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskControllerController : ControllerBase
    {
        private readonly ITaskControllerService _taskControllerService;

        public TaskControllerController(ITaskControllerService taskControllerService)
        {
            _taskControllerService = taskControllerService;
        }
      

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _taskControllerService.GetTaskControllerByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await _taskControllerService.GetAllTaskControllersAsync();
            return Ok(result);
        }       

    }
}