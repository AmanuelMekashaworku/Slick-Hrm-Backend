using slick.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public FileStorageService(string storagePath)
    {
        _storagePath = storagePath;
    }


    public async Task<string> SaveFileAsync(IFormFile file, string folder = "Uploads")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        // Ensure the folder is safe (e.g., no ".." or slashes)
        folder = folder.Replace("\\", "").Replace("/", "").Replace("..", "").Trim();
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadDir = Path.Combine(_storagePath, folder);

        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return the public-accessible URL path (frontend uses this)
        return $"/Files/{folder}/{fileName}";
    }

    public bool DeleteFile(string fileUrl, string folder = "Uploads")
    {
        if (string.IsNullOrEmpty(fileUrl))
            return false;

        try
        {
            // Ensure the folder is safe
            folder = folder.Replace("\\", "").Replace("/", "").Replace("..", "").Trim();
            var fileName = Path.GetFileName(fileUrl);
            var fileDir = Path.Combine(_storagePath, folder);
            var filePath = Path.Combine(fileDir, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public byte[]? GetFileBytes(string fileUrl, string folder = "Uploads")
    {
        if (string.IsNullOrEmpty(fileUrl))
            return null;

        try
        {
            folder = folder.Replace("\\", "").Replace("/", "").Replace("..", "").Trim();
            var fileName = Path.GetFileName(fileUrl);
            var fileDir = Path.Combine(_storagePath, folder);
            var filePath = Path.Combine(fileDir, fileName);

            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
