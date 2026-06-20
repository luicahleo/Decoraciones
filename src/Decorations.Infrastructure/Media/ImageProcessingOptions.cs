namespace Decorations.Infrastructure.Media
{
    public class ImageProcessingOptions
    {
        public int MaxWidthPixels { get; set; } = 1200;
        public int MaxHeightPixels { get; set; } = 1200;
        public long MaxFileSizeBytes { get; set; } = 5242880;
        public int WebPQuality { get; set; } = 80;
    }
}
