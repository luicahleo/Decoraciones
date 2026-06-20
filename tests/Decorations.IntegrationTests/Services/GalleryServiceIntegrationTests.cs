using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Application.Services;
using Decorations.Domain.Entities;
using Decorations.Domain.Enums;
using Decorations.Infrastructure.Persistence;
using Decorations.Infrastructure.Repositories;
using Decorations.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Decorations.IntegrationTests.Services
{
    public class GalleryServiceIntegrationTests
    {
        private readonly Mock<IImageProcessingService> mockImageProcessingService;
        private readonly Mock<IFileStorageService> mockFileStorageService;

        public GalleryServiceIntegrationTests()
        {
            this.mockImageProcessingService = new Mock<IImageProcessingService>();
            this.mockFileStorageService = new Mock<IFileStorageService>();
        }

        [Fact]
        public async Task CreateGalleryItemAsync_PersistsItemToDatabase()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            GalleryItemDto createDto = new GalleryItemDto
            {
                Title = "Fiesta de cumpleaños",
                Description = "Cumpleaños infantil con tema de princesas",
                EventType = "Cumpleaños",
                IsActive = true,
                DisplayOrder = 1
            };

            GalleryItemDto result = await service.CreateGalleryItemAsync(createDto);

            Assert.NotEqual(0, result.Id);
            Assert.Equal("Fiesta de cumpleaños", result.Title);
            Assert.Equal("Cumpleaños infantil con tema de princesas", result.Description);
        }

        [Fact]
        public async Task CreateGalleryItemAsync_WithOnlyTitle_PersistsSuccessfully()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            GalleryItemDto createDto = new GalleryItemDto
            {
                Title = "Elemento mínimo",
                Description = null,
                EventType = null,
                IsActive = false,
                DisplayOrder = 0
            };

            GalleryItemDto result = await service.CreateGalleryItemAsync(createDto);

            Assert.NotEqual(0, result.Id);
            Assert.Equal("Elemento mínimo", result.Title);
            Assert.Null(result.Description);
            Assert.Null(result.EventType);
        }

        [Fact]
        public async Task GetGalleryItemByIdAsync_RetrievesPersistedItem()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryItem item = new GalleryItem
            {
                Title = "Boda de ensueño",
                Description = "Decoración floral completa",
                EventType = "Boda",
                IsActive = true,
                DisplayOrder = 5,
                CreatedAt = DateTime.UtcNow,
                MediaAssets = new List<MediaAsset>()
            };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            GalleryItemDto result = await service.GetGalleryItemByIdAsync(item.Id);

            Assert.NotNull(result);
            Assert.Equal(item.Id, result.Id);
            Assert.Equal("Boda de ensueño", result.Title);
            Assert.Equal("Decoración floral completa", result.Description);
        }

        [Fact]
        public async Task GetAllGalleryItemsAsync_ReturnsMultipleItems()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            await context.GalleryItems.AddRangeAsync(
                new GalleryItem { Title = "Cumpleaños 1", IsActive = true, CreatedAt = DateTime.UtcNow, MediaAssets = new List<MediaAsset>() },
                new GalleryItem { Title = "Cumpleaños 2", IsActive = true, CreatedAt = DateTime.UtcNow, MediaAssets = new List<MediaAsset>() },
                new GalleryItem { Title = "Inactivo", IsActive = false, CreatedAt = DateTime.UtcNow, MediaAssets = new List<MediaAsset>() });
            await context.SaveChangesAsync();

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            IReadOnlyList<GalleryItemDto> result = await service.GetAllGalleryItemsAsync();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllActiveGalleryItemsAsync_ReturnsOnlyActiveItems()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            await context.GalleryItems.AddRangeAsync(
                new GalleryItem { Title = "Activo 1", IsActive = true, CreatedAt = DateTime.UtcNow, MediaAssets = new List<MediaAsset>() },
                new GalleryItem { Title = "Activo 2", IsActive = true, CreatedAt = DateTime.UtcNow, MediaAssets = new List<MediaAsset>() },
                new GalleryItem { Title = "Inactivo", IsActive = false, CreatedAt = DateTime.UtcNow, MediaAssets = new List<MediaAsset>() });
            await context.SaveChangesAsync();

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            IReadOnlyList<GalleryItemDto> result = await service.GetAllActiveGalleryItemsAsync();

            Assert.Equal(2, result.Count);
            Assert.All(result, item => Assert.True(item.IsActive));
        }

        [Fact]
        public async Task UpdateGalleryItemAsync_PersistsChangesToDatabase()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryItem item = new GalleryItem
            {
                Title = "Título original",
                Description = "Descripción original",
                EventType = "Bautizo",
                IsActive = true,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                MediaAssets = new List<MediaAsset>()
            };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            GalleryItemDto updateDto = new GalleryItemDto
            {
                Id = item.Id,
                Title = "Título actualizado",
                Description = "Descripción actualizada",
                EventType = "Boda",
                IsActive = false,
                DisplayOrder = 10
            };

            await service.UpdateGalleryItemAsync(updateDto);

            GalleryItem? retrievedItem = await context.GalleryItems.FindAsync(item.Id);
            Assert.NotNull(retrievedItem);
            Assert.Equal("Título actualizado", retrievedItem.Title);
            Assert.Equal("Descripción actualizada", retrievedItem.Description);
            Assert.Equal("Boda", retrievedItem.EventType);
            Assert.False(retrievedItem.IsActive);
            Assert.Equal(10, retrievedItem.DisplayOrder);
        }

        [Fact]
        public async Task UpdateGalleryItemAsync_CanClearOptionalFields()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryItem item = new GalleryItem
            {
                Title = "Fiesta",
                Description = "Descripción inicial",
                EventType = "Cumpleaños",
                IsActive = true,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                MediaAssets = new List<MediaAsset>()
            };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            GalleryItemDto updateDto = new GalleryItemDto
            {
                Id = item.Id,
                Title = "Fiesta",
                Description = null,
                EventType = null,
                IsActive = true,
                DisplayOrder = 0
            };

            await service.UpdateGalleryItemAsync(updateDto);

            GalleryItem? retrievedItem = await context.GalleryItems.FindAsync(item.Id);
            Assert.NotNull(retrievedItem);
            Assert.Null(retrievedItem.Description);
            Assert.Null(retrievedItem.EventType);
        }

        [Fact]
        public async Task DeleteGalleryItemAsync_RemovesItemFromDatabase()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryItem item = new GalleryItem
            {
                Title = "Elemento a eliminar",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                MediaAssets = new List<MediaAsset>()
            };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();
            int itemId = item.Id;

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            await service.DeleteGalleryItemAsync(itemId);

            GalleryItem? retrievedItem = await context.GalleryItems.FindAsync(itemId);
            Assert.Null(retrievedItem);
        }

        [Fact]
        public async Task DeleteGalleryItemAsync_RemovesAssociatedMediaAssets()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryItem item = new GalleryItem
            {
                Title = "Elemento con media",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                MediaAssets = new List<MediaAsset>()
            };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();

            MediaAsset media = new MediaAsset
            {
                GalleryItemId = item.Id,
                MediaType = MediaType.Image,
                FilePath = "/uploads/test.webp",
                DisplayOrder = 0
            };
            await context.MediaAssets.AddAsync(media);
            await context.SaveChangesAsync();

            this.mockFileStorageService
                .Setup(s => s.DeleteAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            await service.DeleteGalleryItemAsync(item.Id);

            GalleryItem? retrievedItem = await context.GalleryItems.FindAsync(item.Id);
            Assert.Null(retrievedItem);
            int mediaCount = await context.MediaAssets.CountAsync(m => m.GalleryItemId == item.Id);
            Assert.Equal(0, mediaCount);
        }

        [Fact]
        public async Task CreateMultipleItems_AllPersistIndependently()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            GalleryItemDto[] dtos = new[]
            {
                new GalleryItemDto { Title = "Fiesta 1", EventType = "Cumpleaños", IsActive = true, DisplayOrder = 0 },
                new GalleryItemDto { Title = "Fiesta 2", EventType = "Boda", IsActive = true, DisplayOrder = 1 },
                new GalleryItemDto { Title = "Fiesta 3", EventType = null, IsActive = false, DisplayOrder = 2 }
            };

            foreach (GalleryItemDto dto in dtos)
            {
                await service.CreateGalleryItemAsync(dto);
            }

            IReadOnlyList<GalleryItemDto> result = await service.GetAllGalleryItemsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("Fiesta 1", result[0].Title);
            Assert.Equal("Fiesta 2", result[1].Title);
            Assert.Equal("Fiesta 3", result[2].Title);
            Assert.Null(result[2].EventType);
        }

        [Fact]
        public async Task AddVideoToGalleryItemAsync_PersistsYoutubeVideo()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            GalleryItem item = new GalleryItem
            {
                Title = "Evento con video",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                MediaAssets = new List<MediaAsset>()
            };
            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();

            GalleryRepository repository = new GalleryRepository(context);
            GalleryService service = new GalleryService(
                repository,
                new Repository<MediaAsset>(context),
                this.mockImageProcessingService.Object,
                this.mockFileStorageService.Object);

            await service.AddVideoToGalleryItemAsync(item.Id, "dQw4w9WgXcQ", "Video de prueba");

            MediaAsset? media = await context.MediaAssets.FirstOrDefaultAsync(m =>
                m.GalleryItemId == item.Id && m.MediaType == MediaType.Video);

            Assert.NotNull(media);
            Assert.Equal("dQw4w9WgXcQ", media.YoutubeVideoId);
            Assert.Equal("Video de prueba", media.AltText);
        }

        [Fact]
        public async Task GalleryItem_WithNullableFields_SavesCorrectly()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();

            GalleryItem item = new GalleryItem
            {
                Title = "Fiesta minimalista",
                Description = null,
                EventType = null,
                IsActive = true,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                MediaAssets = new List<MediaAsset>()
            };

            await context.GalleryItems.AddAsync(item);
            await context.SaveChangesAsync();

            GalleryItem? retrieved = await context.GalleryItems.FindAsync(item.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("Fiesta minimalista", retrieved.Title);
            Assert.Null(retrieved.Description);
            Assert.Null(retrieved.EventType);
        }
    }
}
