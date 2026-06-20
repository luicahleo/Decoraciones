using Decorations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decorations.Infrastructure.Persistence.Configurations
{
    public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
    {
        public void Configure(EntityTypeBuilder<MediaAsset> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.MediaType).IsRequired();
            builder.Property(m => m.ThumbnailPath).HasMaxLength(500);
            builder.Property(m => m.FullSizePath).HasMaxLength(500);
            builder.Property(m => m.YoutubeVideoId).HasMaxLength(50);
            builder.Property(m => m.AltText).HasMaxLength(200);
            builder.Property(m => m.IsFeatured).HasDefaultValue(false);
        }
    }
}
