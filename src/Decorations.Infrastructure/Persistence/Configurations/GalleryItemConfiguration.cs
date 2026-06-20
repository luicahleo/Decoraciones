using Decorations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decorations.Infrastructure.Persistence.Configurations
{
    public class GalleryItemConfiguration : IEntityTypeConfiguration<GalleryItem>
    {
        public void Configure(EntityTypeBuilder<GalleryItem> builder)
        {
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Title).IsRequired().HasMaxLength(200);
            builder.Property(g => g.Description).HasMaxLength(1000);
            builder.Property(g => g.EventType).HasMaxLength(100);
            builder.Property(g => g.CreatedAt).IsRequired();

            builder.HasMany(g => g.MediaAssets)
                   .WithOne(m => m.GalleryItem)
                   .HasForeignKey(m => m.GalleryItemId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
