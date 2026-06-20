namespace Decorations.Application.DTOs
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconCssClass { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public string SeoMetaTitle { get; set; } = string.Empty;
        public string SeoMetaDescription { get; set; } = string.Empty;
        public string SeoOpenGraphImageUrl { get; set; } = string.Empty;
    }
}
