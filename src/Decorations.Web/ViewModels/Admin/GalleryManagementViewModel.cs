using Decorations.Application.DTOs;

namespace Decorations.Web.ViewModels.Admin
{
    public class GalleryManagementViewModel
    {
        public IReadOnlyList<GalleryItemDto> GalleryItems { get; set; } = new List<GalleryItemDto>();
        public int UnreadMessagesCount { get; set; }
    }

    public class GalleryItemFormViewModel
    {
        public GalleryItemDto Item { get; set; } = new GalleryItemDto();
        public string YoutubeVideoId { get; set; } = string.Empty;
        public string VideoAltText { get; set; } = string.Empty;
    }
}
