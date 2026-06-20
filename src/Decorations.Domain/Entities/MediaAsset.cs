using Decorations.Domain.Enums;

namespace Decorations.Domain.Entities
{
    public class MediaAsset
    {
        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public MediaType MediaType { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string YoutubeVideoId { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public GalleryItem GalleryItem { get; set; } = null!;
    }
}
