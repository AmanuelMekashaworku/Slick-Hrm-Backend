using slick.Application.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace slick.Infrastructure.Services
{
    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly string _connectionString;
        private readonly string _backupDirectory;

        public DatabaseBackupService(
            IConfiguration configuration,
           string backupDirectory)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            _backupDirectory = backupDirectory
                 ?? throw new InvalidOperationException("Backup directory is not configured.");
        }

        public async Task BackupAsync()
        {
            var dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;

            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }

            var backupFileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var fullPath = Path.Combine(_backupDirectory, backupFileName);

            var sql = $@"
            BACKUP DATABASE [{dbName}]
            TO DISK = N'{fullPath}'
            WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10;
            ";

            await ExecuteSqlAsync(sql);
        }
        public async Task RestoreAsync(string backupFilePath)
        {
            var path = Path.IsPathRooted(backupFilePath)
           ? backupFilePath
           : Path.Combine(_backupDirectory, backupFilePath);

            if (!File.Exists(path))
                throw new FileNotFoundException("Backup file not found.", path);

            var dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;

      
            // Connect to 'master' DB for restore
            var masterConnectionString = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString;

            var sql = $@"
                            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            RESTORE DATABASE [{dbName}]
                            FROM DISK = N'{backupFilePath}'
                            WITH REPLACE;
                            ALTER DATABASE [{dbName}] SET MULTI_USER;
                        ";

            await ExecuteSqlWithConnectionAsync(sql, masterConnectionString);
        }
        private async Task ExecuteSqlWithConnectionAsync(string sql, string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        private async Task ExecuteSqlAsync(string sql)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
                        
        // This is the updated method that resolves CS8619 warning
        public List<string> GetAvailableBackups()
        {
            if (!Directory.Exists(_backupDirectory))
            {
                throw new DirectoryNotFoundException("Backup directory not found.");
            }

            var backups = Directory.GetFiles(_backupDirectory, "*.bak")
                                   .Select(filePath => Path.GetFileName(filePath))  // Get only the file names
                                   .Where(fileName => fileName != null)  // Ensure fileName is not null
                                   .ToList();  // This will be a List<string> (no nulls)

            return backups;
        }
        public string BackupDirectory => _backupDirectory;

    }
}
