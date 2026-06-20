using Decorations.Domain.Entities;
using Decorations.Infrastructure.Persistence;
using Decorations.Infrastructure.Repositories;
using Decorations.IntegrationTests.Helpers;

namespace Decorations.IntegrationTests.Repositories
{
    public class GalleryRepositoryTests
    {
        [Fact]
        public async Task GetAllWithMediaAsync_ReturnsGalleryItemsWithMediaAssets()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);
            GalleryItem item = new GalleryItem { Title = "Fiesta", IsActive = true };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();
            await context.MediaAssets.AddAsync(new MediaAsset
            {
                GalleryItemId = item.Id,
                MediaType = Domain.Enums.MediaType.Image,
                ThumbnailPath = "/uploads/events/1/thumbnails/test.webp",
                FullSizePath = "/uploads/events/1/full-size/test.webp"
            });
            await context.SaveChangesAsync();

            IReadOnlyList<GalleryItem> result = await repository.GetAllWithMediaAsync();

            Assert.Single(result.First().MediaAssets);
        }

        [Fact]
        public async Task GetAllActiveWithMediaAsync_ReturnsOnlyActiveItems()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);
            await context.GalleryItems.AddRangeAsync(
                new GalleryItem { Title = "Activo", IsActive = true },
                new GalleryItem { Title = "Inactivo", IsActive = false });
            await context.SaveChangesAsync();

            IReadOnlyList<GalleryItem> result = await repository.GetAllActiveWithMediaAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllActiveWithMediaAsync_WhenActiveItem_ReturnsTitleCorrectly()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);
            await context.GalleryItems.AddAsync(new GalleryItem { Title = "Bautizo Especial", IsActive = true });
            await context.SaveChangesAsync();

            IReadOnlyList<GalleryItem> result = await repository.GetAllActiveWithMediaAsync();

            Assert.Equal("Bautizo Especial", result.First().Title);
        }

        [Fact]
        public async Task GetByIdWithMediaAsync_WhenItemExists_ReturnsItemWithMedia()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);
            GalleryItem item = new GalleryItem { Title = "Con Media", IsActive = true };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();
            await context.MediaAssets.AddAsync(new MediaAsset
            {
                GalleryItemId = item.Id,
                MediaType = Domain.Enums.MediaType.Video,
                YoutubeVideoId = "abc123"
            });
            await context.SaveChangesAsync();

            GalleryItem? result = await repository.GetByIdWithMediaAsync(item.Id);

            Assert.Single(result!.MediaAssets);
        }

        [Fact]
        public async Task GetByIdWithMediaAsync_WhenItemNotFound_ReturnsNull()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);

            GalleryItem? result = await repository.GetByIdWithMediaAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllWithMediaAsync_WhenNoItems_ReturnsEmptyList()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);

            IReadOnlyList<GalleryItem> result = await repository.GetAllWithMediaAsync();

            Assert.Empty(result);
        }
    }
}
