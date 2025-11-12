using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace slick.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseBackupService _backupService;

        public DatabaseController(IDatabaseBackupService backupService)
        {
            _backupService = backupService;
        }

        // Endpoint to initiate the backup process
        [HttpPost("backup")]
        public async Task<IActionResult> Backup()
        {
            try
            {
                await _backupService.BackupAsync();
                return Ok(new { success = true, message = "Backup completed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
        [HttpPost("restore")]
        public async Task<IActionResult> Restore([FromQuery] string backupFileName)
        {
            if (string.IsNullOrEmpty(backupFileName))
            {
                return BadRequest(new { success = false, error = "Backup file name is required." });
            }

            // Get available backups and validate that the file exists
            var availableBackups = _backupService.GetAvailableBackups();

            var matchedFile = availableBackups
                .FirstOrDefault(file => file.Equals(backupFileName, StringComparison.OrdinalIgnoreCase));

            if (matchedFile == null)
            {
                return NotFound(new { success = false, error = "Backup file does not exist." });
            }

            // Construct the full path
            var backupFilePath = Path.Combine(_backupService.BackupDirectory, matchedFile);

            try
            {
                await _backupService.RestoreAsync(backupFilePath);
                return Ok(new { success = true, message = "Restore completed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // Endpoint to list all available backup files
        [HttpGet("backups")]
        public IActionResult GetAvailableBackups()
        {
            try
            {
                var backups = _backupService.GetAvailableBackups();
                return Ok(new { success = true, backups });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
