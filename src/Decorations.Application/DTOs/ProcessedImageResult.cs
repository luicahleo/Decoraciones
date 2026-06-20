namespace Decorations.Application.DTOs
{
    public class ProcessedImageResult
    {
        public byte[] ThumbnailBytes { get; set; } = Array.Empty<byte>();
        public byte[] FullSizeBytes { get; set; } = Array.Empty<byte>();
    }
}
