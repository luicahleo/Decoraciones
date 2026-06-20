using System.ComponentModel.DataAnnotations;

namespace Decorations.Application.DTOs
{
    public class GalleryItemDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? EventType { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public bool ShowAsGrid { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyList<MediaAssetDto> MediaAssets { get; set; } = new List<MediaAssetDto>();
    }
}
