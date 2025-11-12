using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IFileStorageService
    {
        bool DeleteFile(string fileUrl, string folder = "Uploads");
        Task<string> SaveFileAsync(IFormFile file, string folder = "uploads");
    }
}
