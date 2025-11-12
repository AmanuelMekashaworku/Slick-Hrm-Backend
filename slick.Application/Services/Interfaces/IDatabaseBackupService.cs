using System.Collections.Generic;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IDatabaseBackupService
    {
        Task BackupAsync();
        Task RestoreAsync(string backupFilePath);
        List<string> GetAvailableBackups();  // Method to get available backup files
        string BackupDirectory { get; } // ✅ Add this
    }
}
