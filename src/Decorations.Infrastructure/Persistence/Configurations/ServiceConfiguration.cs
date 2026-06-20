using Decorations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decorations.Infrastructure.Persistence.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Description).IsRequired().HasMaxLength(2000);
            builder.Property(s => s.IconCssClass).HasMaxLength(100);
            builder.Property(s => s.SeoMetaTitle).HasMaxLength(60);
            builder.Property(s => s.SeoMetaDescription).HasMaxLength(160);
            builder.Property(s => s.SeoOpenGraphImageUrl).HasMaxLength(500);
        }
    }
}
