using Microsoft.AspNetCore.Http;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IFileService
    {
        Task<string?> SaveImageAsync(IFormFile file, string folder);
        void DeleteImage(string? filePath);
    }
}