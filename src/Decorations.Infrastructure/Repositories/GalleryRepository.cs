using Decorations.Application.Interfaces;
using Decorations.Domain.Entities;
using Decorations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Decorations.Infrastructure.Repositories
{
    public class GalleryRepository : Repository<GalleryItem>, IGalleryRepository
    {
        private readonly ApplicationDbContext context;

        public GalleryRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IReadOnlyList<GalleryItem>> GetAllWithMediaAsync()
        {
            return await this.context.GalleryItems
                .Include(g => g.MediaAssets)
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<GalleryItem>> GetAllActiveWithMediaAsync()
        {
            return await this.context.GalleryItems
                .Where(g => g.IsActive)
                .Include(g => g.MediaAssets)
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();
        }

        public async Task<GalleryItem?> GetByIdWithMediaAsync(int id)
        {
            return await this.context.GalleryItems
                .Include(g => g.MediaAssets)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
    }
}
