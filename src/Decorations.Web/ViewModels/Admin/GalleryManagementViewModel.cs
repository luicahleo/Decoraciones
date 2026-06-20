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
        
        // Para soporte multi-upload de imágenes
        public List<MediaFileViewModel> UploadedFiles { get; set; } = new();
    }

    public class MediaFileViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Base64Preview { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
    }
}
