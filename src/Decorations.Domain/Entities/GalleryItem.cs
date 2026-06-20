namespace Decorations.Domain.Entities
{
    public class GalleryItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? EventType { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    }
}
