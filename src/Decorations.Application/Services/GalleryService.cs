using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Domain.Entities;
using Decorations.Domain.Enums;

namespace Decorations.Application.Services
{
    public class GalleryService : IGalleryService
    {
        private readonly IGalleryRepository galleryRepository;
        private readonly IRepository<MediaAsset> mediaAssetRepository;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IFileStorageService fileStorageService;

        public GalleryService(
            IGalleryRepository galleryRepository,
            IRepository<MediaAsset> mediaAssetRepository,
            IImageProcessingService imageProcessingService,
            IFileStorageService fileStorageService)
        {
            this.galleryRepository = galleryRepository;
            this.mediaAssetRepository = mediaAssetRepository;
            this.imageProcessingService = imageProcessingService;
            this.fileStorageService = fileStorageService;
        }

        public async Task<IReadOnlyList<GalleryItemDto>> GetAllActiveGalleryItemsAsync()
        {
            IReadOnlyList<GalleryItem> items = await this.galleryRepository.GetAllActiveWithMediaAsync();
            return items.Select(g => MapToDto(g)).ToList();
        }

        public async Task<IReadOnlyList<GalleryItemDto>> GetAllGalleryItemsAsync()
        {
            IReadOnlyList<GalleryItem> items = await this.galleryRepository.GetAllWithMediaAsync();
            return items.Select(g => MapToDto(g)).ToList();
        }

        public async Task<GalleryItemDto?> GetGalleryItemByIdAsync(int id)
        {
            GalleryItem? item = await this.galleryRepository.GetByIdWithMediaAsync(id);
            return item != null ? MapToDto(item) : null;
        }

        public async Task<GalleryItemDto> CreateGalleryItemAsync(GalleryItemDto dto)
        {
            GalleryItem item = MapToNewEntity(dto);
            await this.galleryRepository.AddAsync(item);
            await this.galleryRepository.SaveChangesAsync();
            return MapToDto(item);
        }

        public async Task UpdateGalleryItemAsync(GalleryItemDto dto)
        {
            GalleryItem? item = await this.galleryRepository.GetByIdAsync(dto.Id);
            if (item == null)
            {
                return;
            }

            UpdateEntityFromDto(item, dto);
            this.galleryRepository.Update(item);
            await this.galleryRepository.SaveChangesAsync();
        }

        public async Task DeleteGalleryItemAsync(int id)
        {
            GalleryItem? item = await this.galleryRepository.GetByIdWithMediaAsync(id);
            if (item == null)
            {
                return;
            }

            await this.DeletePhysicalImagesAsync(item.MediaAssets);
            this.galleryRepository.Delete(item);
            await this.galleryRepository.SaveChangesAsync();
        }

        public async Task<MediaAssetDto> AddImageToGalleryItemAsync(int galleryItemId, Stream imageStream, string fileName, string altText)
        {
            byte[] processedBytes = await this.imageProcessingService.ProcessImageAsync(imageStream, fileName);
            string webpFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.webp";
            string relativePath = await this.fileStorageService.SaveAsync(processedBytes, webpFileName);

            MediaAsset asset = new MediaAsset
            {
                GalleryItemId = galleryItemId,
                MediaType = MediaType.Image,
                FilePath = relativePath,
                AltText = altText
            };

            await this.mediaAssetRepository.AddAsync(asset);
            await this.mediaAssetRepository.SaveChangesAsync();
            return MapAssetToDto(asset);
        }

        public async Task<MediaAssetDto> AddVideoToGalleryItemAsync(int galleryItemId, string youtubeVideoId, string altText)
        {
            MediaAsset asset = new MediaAsset
            {
                GalleryItemId = galleryItemId,
                MediaType = MediaType.Video,
                YoutubeVideoId = youtubeVideoId,
                AltText = altText
            };

            await this.mediaAssetRepository.AddAsync(asset);
            await this.mediaAssetRepository.SaveChangesAsync();
            return MapAssetToDto(asset);
        }

        public async Task DeleteMediaAssetAsync(int mediaAssetId)
        {
            MediaAsset? asset = await this.mediaAssetRepository.GetByIdAsync(mediaAssetId);
            if (asset == null)
            {
                return;
            }

            if (asset.MediaType == MediaType.Image && !string.IsNullOrWhiteSpace(asset.FilePath))
            {
                await this.fileStorageService.DeleteAsync(asset.FilePath);
            }

            this.mediaAssetRepository.Delete(asset);
            await this.mediaAssetRepository.SaveChangesAsync();
        }

        private async Task DeletePhysicalImagesAsync(ICollection<MediaAsset> mediaAssets)
        {
            IEnumerable<Task> deleteTasks = mediaAssets
                .Where(m => m.MediaType == MediaType.Image && !string.IsNullOrWhiteSpace(m.FilePath))
                .Select(m => this.fileStorageService.DeleteAsync(m.FilePath));

            await Task.WhenAll(deleteTasks);
        }

        private static GalleryItemDto MapToDto(GalleryItem item)
        {
            return new GalleryItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                EventType = item.EventType,
                IsActive = item.IsActive,
                DisplayOrder = item.DisplayOrder,
                CreatedAt = item.CreatedAt,
                MediaAssets = item.MediaAssets.Select(m => MapAssetToDto(m)).ToList()
            };
        }

        private static GalleryItem MapToNewEntity(GalleryItemDto dto)
        {
            return new GalleryItem
            {
                Title = dto.Title,
                Description = dto.Description,
                EventType = dto.EventType,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static void UpdateEntityFromDto(GalleryItem entity, GalleryItemDto dto)
        {
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.EventType = dto.EventType;
            entity.IsActive = dto.IsActive;
            entity.DisplayOrder = dto.DisplayOrder;
        }

        private static MediaAssetDto MapAssetToDto(MediaAsset asset)
        {
            return new MediaAssetDto
            {
                Id = asset.Id,
                GalleryItemId = asset.GalleryItemId,
                MediaType = asset.MediaType,
                FilePath = asset.FilePath,
                YoutubeVideoId = asset.YoutubeVideoId,
                AltText = asset.AltText,
                DisplayOrder = asset.DisplayOrder
            };
        }
    }
}
