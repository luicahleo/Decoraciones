using Decorations.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Decorations.Infrastructure.Storage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<FileStorageService> logger;

        public FileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<FileStorageService> logger)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
        }

        public async Task<string> SaveAsync(byte[] content, string fileName)
        {
            string uploadsDirectory = Path.Combine(this.webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            string uniqueFileName = $"{Guid.NewGuid():N}_{fileName}";
            string physicalFilePath = Path.Combine(uploadsDirectory, uniqueFileName);

            await File.WriteAllBytesAsync(physicalFilePath, content);

            this.logger.LogDebug("FileStorageService.SaveAsync - Archivo guardado: {FileName}", uniqueFileName);

            return $"/uploads/{uniqueFileName}";
        }

        public Task DeleteAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.CompletedTask;
            }

            string physicalPath = Path.Combine(
                this.webHostEnvironment.WebRootPath,
                relativePath.TrimStart('/'));

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                this.logger.LogDebug("FileStorageService.DeleteAsync - Archivo eliminado: {RelativePath}", relativePath);
            }

            return Task.CompletedTask;
        }
    }
}
