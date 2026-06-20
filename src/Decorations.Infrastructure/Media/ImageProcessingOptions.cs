namespace Decorations.Infrastructure.Media
{
    public class ImageProcessingOptions
    {
        public long MaxFileSizeBytes { get; set; } = 8388608; // 8 MB
        public ThumbnailOptions Thumbnail { get; set; } = new ThumbnailOptions();
        public FullSizeOptions FullSize { get; set; } = new FullSizeOptions();
    }

    public class ThumbnailOptions
    {
        public int MaxWidthPixels { get; set; } = 600;
        public int MaxHeightPixels { get; set; } = 600;
        public int WebPQuality { get; set; } = 70;
    }

    public class FullSizeOptions
    {
        public int MaxWidthPixels { get; set; } = 1400;
        public int MaxHeightPixels { get; set; } = 1400;
        public int WebPQuality { get; set; } = 85;
    }
}
