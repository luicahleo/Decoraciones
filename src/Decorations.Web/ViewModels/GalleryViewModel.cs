using Decorations.Application.DTOs;

namespace Decorations.Web.ViewModels
{
    public class GalleryViewModel : SeoViewModel
    {
        public IReadOnlyList<GalleryItemDto> GalleryItems { get; set; } = new List<GalleryItemDto>();
    }
}
