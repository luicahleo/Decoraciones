namespace Decorations.Application.Interfaces
{
    public interface IImageProcessingService
    {
        Task<byte[]> ProcessImageAsync(Stream imageStream, string originalFileName);
    }
}
