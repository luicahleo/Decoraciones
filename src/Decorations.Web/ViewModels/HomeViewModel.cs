using Decorations.Application.DTOs;

namespace Decorations.Web.ViewModels
{
    public class HomeViewModel : SeoViewModel
    {
        public IReadOnlyList<ServiceDto> Services { get; set; } = new List<ServiceDto>();
        public IReadOnlyList<GalleryItemDto> GalleryPreview { get; set; } = new List<GalleryItemDto>();
        public ContactSettingsDto ContactSettings { get; set; } = new ContactSettingsDto();
    }
}
