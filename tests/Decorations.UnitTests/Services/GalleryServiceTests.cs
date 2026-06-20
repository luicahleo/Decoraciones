using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Application.Services;
using Decorations.Domain.Entities;
using Decorations.Domain.Enums;
using Moq;

namespace Decorations.UnitTests.Services
{
    public class GalleryServiceTests
    {
        private readonly Mock<IGalleryRepository> galleryRepositoryMock;
        private readonly Mock<IRepository<MediaAsset>> mediaAssetRepositoryMock;
        private readonly Mock<IImageProcessingService> imageProcessingServiceMock;
        private readonly Mock<IFileStorageService> fileStorageServiceMock;
        private readonly GalleryService service;

        public GalleryServiceTests()
        {
            this.galleryRepositoryMock = new Mock<IGalleryRepository>();
            this.mediaAssetRepositoryMock = new Mock<IRepository<MediaAsset>>();
            this.imageProcessingServiceMock = new Mock<IImageProcessingService>();
            this.fileStorageServiceMock = new Mock<IFileStorageService>();

            this.service = new GalleryService(
                this.galleryRepositoryMock.Object,
                this.mediaAssetRepositoryMock.Object,
                this.imageProcessingServiceMock.Object,
                this.fileStorageServiceMock.Object);
        }

        [Fact]
        public async Task GetAllActiveGalleryItemsAsync_WhenItemsExist_ReturnsCorrectCount()
        {
            IReadOnlyList<GalleryItem> items = new List<GalleryItem>
            {
                new GalleryItem { Id = 1, Title = "Fiesta 1", IsActive = true, MediaAssets = new List<MediaAsset>() },
                new GalleryItem { Id = 2, Title = "Fiesta 2", IsActive = true, MediaAssets = new List<MediaAsset>() }
            };
            this.galleryRepositoryMock.Setup(r => r.GetAllActiveWithMediaAsync()).ReturnsAsync(items);

            IReadOnlyList<GalleryItemDto> result = await this.service.GetAllActiveGalleryItemsAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllActiveGalleryItemsAsync_WhenItemsExist_MapsTitleCorrectly()
        {
            IReadOnlyList<GalleryItem> items = new List<GalleryItem>
            {
                new GalleryItem { Id = 1, Title = "Cumpleaños mágico", IsActive = true, MediaAssets = new List<MediaAsset>() }
            };
            this.galleryRepositoryMock.Setup(r => r.GetAllActiveWithMediaAsync()).ReturnsAsync(items);

            IReadOnlyList<GalleryItemDto> result = await this.service.GetAllActiveGalleryItemsAsync();

            Assert.Equal("Cumpleaños mágico", result.First().Title);
        }

        [Fact]
        public async Task GetGalleryItemByIdAsync_WhenNotFound_ReturnsNull()
        {
            this.galleryRepositoryMock.Setup(r => r.GetByIdWithMediaAsync(999)).ReturnsAsync((GalleryItem?)null);

            GalleryItemDto? result = await this.service.GetGalleryItemByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddImageToGalleryItemAsync_CallsImageProcessingService()
        {
            byte[] processedBytes = new byte[] { 1, 2, 3 };
            this.imageProcessingServiceMock
                .Setup(s => s.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(processedBytes);
            this.fileStorageServiceMock
                .Setup(s => s.SaveAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync("/uploads/test.webp");
            this.mediaAssetRepositoryMock.Setup(r => r.AddAsync(It.IsAny<MediaAsset>())).Returns(Task.CompletedTask);
            this.mediaAssetRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            using Stream stream = new MemoryStream(new byte[] { 10, 20, 30 });
            await this.service.AddImageToGalleryItemAsync(1, stream, "foto.jpg", "Alt text");

            this.imageProcessingServiceMock.Verify(s => s.ProcessImageAsync(It.IsAny<Stream>(), "foto.jpg"), Times.Once);
        }

        [Fact]
        public async Task AddImageToGalleryItemAsync_CallsFileStorageService()
        {
            byte[] processedBytes = new byte[] { 1, 2, 3 };
            this.imageProcessingServiceMock
                .Setup(s => s.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(processedBytes);
            this.fileStorageServiceMock
                .Setup(s => s.SaveAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync("/uploads/test.webp");
            this.mediaAssetRepositoryMock.Setup(r => r.AddAsync(It.IsAny<MediaAsset>())).Returns(Task.CompletedTask);
            this.mediaAssetRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            using Stream stream = new MemoryStream(new byte[] { 10, 20, 30 });
            await this.service.AddImageToGalleryItemAsync(1, stream, "foto.jpg", "Alt text");

            this.fileStorageServiceMock.Verify(s => s.SaveAsync(processedBytes, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddImageToGalleryItemAsync_SavesMediaAssetWithImageType()
        {
            MediaAsset? capturedAsset = null;
            this.imageProcessingServiceMock
                .Setup(s => s.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(new byte[] { 1 });
            this.fileStorageServiceMock
                .Setup(s => s.SaveAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync("/uploads/test.webp");
            this.mediaAssetRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<MediaAsset>()))
                .Callback<MediaAsset>(a => capturedAsset = a)
                .Returns(Task.CompletedTask);
            this.mediaAssetRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            using Stream stream = new MemoryStream(new byte[] { 10 });
            await this.service.AddImageToGalleryItemAsync(1, stream, "foto.jpg", "Alt");

            Assert.Equal(MediaType.Image, capturedAsset!.MediaType);
        }

        [Fact]
        public async Task AddVideoToGalleryItemAsync_SavesYoutubeVideoId()
        {
            MediaAsset? capturedAsset = null;
            this.mediaAssetRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<MediaAsset>()))
                .Callback<MediaAsset>(a => capturedAsset = a)
                .Returns(Task.CompletedTask);
            this.mediaAssetRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.AddVideoToGalleryItemAsync(1, "dQw4w9WgXcQ", "Vídeo");

            Assert.Equal("dQw4w9WgXcQ", capturedAsset!.YoutubeVideoId);
        }

        [Fact]
        public async Task AddVideoToGalleryItemAsync_SavesAssetWithVideoType()
        {
            MediaAsset? capturedAsset = null;
            this.mediaAssetRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<MediaAsset>()))
                .Callback<MediaAsset>(a => capturedAsset = a)
                .Returns(Task.CompletedTask);
            this.mediaAssetRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.AddVideoToGalleryItemAsync(1, "abc123", "Vídeo test");

            Assert.Equal(MediaType.Video, capturedAsset!.MediaType);
        }

        [Fact]
        public async Task DeleteGalleryItemAsync_WhenItemNotFound_DoesNotCallDelete()
        {
            this.galleryRepositoryMock.Setup(r => r.GetByIdWithMediaAsync(999)).ReturnsAsync((GalleryItem?)null);

            await this.service.DeleteGalleryItemAsync(999);

            this.galleryRepositoryMock.Verify(r => r.Delete(It.IsAny<GalleryItem>()), Times.Never);
        }

        [Fact]
        public async Task DeleteGalleryItemAsync_WhenItemExists_CallsRepositoryDelete()
        {
            GalleryItem item = new GalleryItem { Id = 1, Title = "Fiesta", MediaAssets = new List<MediaAsset>() };
            this.galleryRepositoryMock.Setup(r => r.GetByIdWithMediaAsync(1)).ReturnsAsync(item);
            this.galleryRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.DeleteGalleryItemAsync(1);

            this.galleryRepositoryMock.Verify(r => r.Delete(item), Times.Once);
        }

        [Fact]
        public async Task DeleteGalleryItemAsync_WhenItemHasImages_CallsFileStorageDelete()
        {
            GalleryItem item = new GalleryItem
            {
                Id = 1,
                Title = "Fiesta",
                MediaAssets = new List<MediaAsset>
                {
                    new MediaAsset { Id = 10, MediaType = MediaType.Image, FilePath = "/uploads/img.webp" }
                }
            };
            this.galleryRepositoryMock.Setup(r => r.GetByIdWithMediaAsync(1)).ReturnsAsync(item);
            this.galleryRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            this.fileStorageServiceMock.Setup(s => s.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            await this.service.DeleteGalleryItemAsync(1);

            this.fileStorageServiceMock.Verify(s => s.DeleteAsync("/uploads/img.webp"), Times.Once);
        }
    }
}
