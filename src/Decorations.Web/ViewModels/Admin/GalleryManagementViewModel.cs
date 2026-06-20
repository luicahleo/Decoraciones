using Decorations.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Decorations.Web.ViewModels.Admin
{
    public class GalleryManagementViewModel
    {
        public IReadOnlyList<GalleryItemDto> GalleryItems { get; set; } = new List<GalleryItemDto>();
        public int UnreadMessagesCount { get; set; }
    }

    public class GalleryItemFormViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        public GalleryItemDto Item { get; set; } = new GalleryItemDto();

        public string? YoutubeVideoId { get; set; }
        public string? VideoAltText { get; set; }
    }
}
