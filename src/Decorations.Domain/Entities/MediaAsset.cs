using Decorations.Domain.Enums;

namespace Decorations.Domain.Entities
{
    public class MediaAsset
    {
        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public MediaType MediaType { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? FullSizePath { get; set; }
        public string? YoutubeVideoId { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsFeatured { get; set; } = false;
        public GalleryItem GalleryItem { get; set; } = null!;
    }
}
