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

        public async Task<string> SaveAsync(byte[] content, string fileName, string basePath = "")
        {
            string uploadsDirectory = Path.Combine(this.webHostEnvironment.WebRootPath, "uploads");

            // Crear directorio de uploads si no existe
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            // Crear subdirectorio con basePath si se proporciona
            string targetDirectory = uploadsDirectory;
            string relativePath = "uploads";

            if (!string.IsNullOrWhiteSpace(basePath))
            {
                targetDirectory = Path.Combine(uploadsDirectory, basePath);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
                relativePath = Path.Combine("uploads", basePath).Replace("\\", "/");
            }

            string uniqueFileName = $"{Guid.NewGuid():N}_{fileName}";
            string physicalFilePath = Path.Combine(targetDirectory, uniqueFileName);

            await File.WriteAllBytesAsync(physicalFilePath, content);

            this.logger.LogDebug(
                "FileStorageService.SaveAsync - Archivo guardado: {FileName} en ruta: {BasePath}",
                uniqueFileName,
                basePath);

            return $"/{relativePath}/{uniqueFileName}";
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
