using Decorations.Application.DTOs;

namespace Decorations.Application.Interfaces
{
    public interface IGalleryService
    {
        Task<IReadOnlyList<GalleryItemDto>> GetAllActiveGalleryItemsAsync();
        Task<IReadOnlyList<GalleryItemDto>> GetAllGalleryItemsAsync();
        Task<GalleryItemDto?> GetGalleryItemByIdAsync(int id);
        Task<GalleryItemDto> CreateGalleryItemAsync(GalleryItemDto dto);
        Task UpdateGalleryItemAsync(GalleryItemDto dto);
        Task UpdateMediaAssetAsync(MediaAssetDto dto);
        Task SetFeaturedMediaAssetAsync(int galleryItemId, int mediaAssetId);
        Task DeleteGalleryItemAsync(int id);
        Task<MediaAssetDto> AddImageToGalleryItemAsync(int galleryItemId, Stream imageStream, string fileName, string altText);
        Task<MediaAssetDto> AddVideoToGalleryItemAsync(int galleryItemId, string youtubeVideoId, string altText);
        Task DeleteMediaAssetAsync(int mediaAssetId);
    }
}
