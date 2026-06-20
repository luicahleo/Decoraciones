using Decorations.Application.DTOs;

namespace Decorations.Application.Interfaces
{
    public interface IImageProcessingService
    {
        /// <summary>
        /// Procesa una imagen y devuelve dos versiones: thumbnail y full-size
        /// </summary>
        Task<ProcessedImageResult> ProcessImageAsync(Stream imageStream, string originalFileName);
    }
}
