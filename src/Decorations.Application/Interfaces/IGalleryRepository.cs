using Decorations.Domain.Entities;

namespace Decorations.Application.Interfaces
{
    public interface IGalleryRepository : IRepository<GalleryItem>
    {
        Task<IReadOnlyList<GalleryItem>> GetAllWithMediaAsync();
        Task<IReadOnlyList<GalleryItem>> GetAllActiveWithMediaAsync();
        Task<GalleryItem?> GetByIdWithMediaAsync(int id);
    }
}
