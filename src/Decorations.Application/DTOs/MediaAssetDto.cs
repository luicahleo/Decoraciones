using Decorations.Domain.Enums;

namespace Decorations.Application.DTOs
{
    public class MediaAssetDto
    {
        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public MediaType MediaType { get; set; }
        public string? FilePath { get; set; }
        public string? YoutubeVideoId { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
    }
}
