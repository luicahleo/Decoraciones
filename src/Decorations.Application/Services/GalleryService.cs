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

            // Insertar al principio: menor DisplayOrder que cualquier colección existente,
            // para que las novedades aparezcan primero sin intervención manual.
            IReadOnlyList<GalleryItem> existing = await this.galleryRepository.GetAllAsync();
            item.DisplayOrder = existing.Count > 0 ? existing.Min(g => g.DisplayOrder) - 1 : 0;

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

        public async Task UpdateMediaAssetAsync(MediaAssetDto dto)
        {
            MediaAsset? asset = await this.mediaAssetRepository.GetByIdAsync(dto.Id);
            if (asset == null)
            {
                return;
            }

            asset.IsFeatured = dto.IsFeatured;
            asset.DisplayOrder = dto.DisplayOrder;
            asset.AltText = dto.AltText;
            
            this.mediaAssetRepository.Update(asset);
            await this.mediaAssetRepository.SaveChangesAsync();
        }

        public async Task SetFeaturedMediaAssetAsync(int galleryItemId, int mediaAssetId)
        {
            // Marca la imagen seleccionada como portada y desmarca las demás de la misma galería.
            GalleryItem? item = await this.galleryRepository.GetByIdWithMediaAsync(galleryItemId);
            if (item == null)
            {
                return;
            }

            foreach (MediaAsset asset in item.MediaAssets)
            {
                asset.IsFeatured = asset.Id == mediaAssetId;
            }

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

        public async Task ReorderGalleryItemsAsync(IReadOnlyList<int> orderedIds)
        {
            if (orderedIds == null || orderedIds.Count == 0)
            {
                return;
            }

            // Reindexa DisplayOrder según la posición recibida: la primera queda en 0, la segunda en 1, etc.
            IReadOnlyList<GalleryItem> items = await this.galleryRepository.GetAllAsync();
            Dictionary<int, GalleryItem> itemsById = items.ToDictionary(i => i.Id);

            for (int index = 0; index < orderedIds.Count; index++)
            {
                if (itemsById.TryGetValue(orderedIds[index], out GalleryItem? item))
                {
                    item.DisplayOrder = index;
                    this.galleryRepository.Update(item);
                }
            }

            await this.galleryRepository.SaveChangesAsync();
        }

        public async Task<MediaAssetDto> AddImageToGalleryItemAsync(int galleryItemId, Stream imageStream, string fileName, string altText)
        {
            // Obtener el GalleryItem para saber el evento (o usar el ID del item como organizador)
            GalleryItem? item = await this.galleryRepository.GetByIdAsync(galleryItemId);
            if (item == null)
            {
                throw new InvalidOperationException($"El elemento de galería con ID {galleryItemId} no existe.");
            }

            // Procesar imagen: genera thumbnail y full-size
            ProcessedImageResult processedResult = await this.imageProcessingService.ProcessImageAsync(imageStream, fileName);
            string webpFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.webp";

            // Ruta base: events/{galleryItemId}
            string basePath = $"events/{galleryItemId}";

            // Guardar thumbnail en carpeta separada
            string thumbnailPath = await this.fileStorageService.SaveAsync(
                processedResult.ThumbnailBytes,
                webpFileName,
                $"{basePath}/thumbnails");

            // Guardar full-size en carpeta separada
            string fullSizePath = await this.fileStorageService.SaveAsync(
                processedResult.FullSizeBytes,
                webpFileName,
                $"{basePath}/full-size");

            MediaAsset asset = new MediaAsset
            {
                GalleryItemId = galleryItemId,
                MediaType = MediaType.Image,
                ThumbnailPath = thumbnailPath,
                FullSizePath = fullSizePath,
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

            if (asset.MediaType == MediaType.Image)
            {
                // Eliminar ambas versiones
                if (!string.IsNullOrWhiteSpace(asset.ThumbnailPath))
                {
                    await this.fileStorageService.DeleteAsync(asset.ThumbnailPath);
                }

                if (!string.IsNullOrWhiteSpace(asset.FullSizePath))
                {
                    await this.fileStorageService.DeleteAsync(asset.FullSizePath);
                }
            }

            this.mediaAssetRepository.Delete(asset);
            await this.mediaAssetRepository.SaveChangesAsync();
        }

        private async Task DeletePhysicalImagesAsync(ICollection<MediaAsset> mediaAssets)
        {
            IEnumerable<Task> deleteTasks = mediaAssets
                .Where(m => m.MediaType == MediaType.Image)
                .SelectMany(m => new[]
                {
                    string.IsNullOrWhiteSpace(m.ThumbnailPath) 
                        ? Task.CompletedTask 
                        : this.fileStorageService.DeleteAsync(m.ThumbnailPath),
                    string.IsNullOrWhiteSpace(m.FullSizePath) 
                        ? Task.CompletedTask 
                        : this.fileStorageService.DeleteAsync(m.FullSizePath)
                });

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
                ShowAsGrid = item.ShowAsGrid,
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
                ShowAsGrid = dto.ShowAsGrid,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static void UpdateEntityFromDto(GalleryItem entity, GalleryItemDto dto)
        {
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.EventType = dto.EventType;
            entity.IsActive = dto.IsActive;
            // DisplayOrder no se modifica al editar: el orden se gestiona al crear
            // (nuevas al principio) y con el reordenamiento drag-and-drop.
            entity.ShowAsGrid = dto.ShowAsGrid;
        }

        private static MediaAssetDto MapAssetToDto(MediaAsset asset)
        {
            return new MediaAssetDto
            {
                Id = asset.Id,
                GalleryItemId = asset.GalleryItemId,
                MediaType = asset.MediaType,
                ThumbnailPath = asset.ThumbnailPath,
                FullSizePath = asset.FullSizePath,
                YoutubeVideoId = asset.YoutubeVideoId,
                AltText = asset.AltText,
                DisplayOrder = asset.DisplayOrder,
                IsFeatured = asset.IsFeatured
            };
        }
    }
}
