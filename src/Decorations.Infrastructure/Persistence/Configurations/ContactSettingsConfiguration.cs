using Decorations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decorations.Infrastructure.Persistence.Configurations
{
    public class ContactSettingsConfiguration : IEntityTypeConfiguration<ContactSettings>
    {
        public void Configure(EntityTypeBuilder<ContactSettings> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.BusinessName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.WhatsAppNumber).HasMaxLength(20);
            builder.Property(c => c.Email).HasMaxLength(200);
            builder.Property(c => c.InstagramUrl).HasMaxLength(500);
            builder.Property(c => c.FacebookUrl).HasMaxLength(500);
            builder.Property(c => c.Address).HasMaxLength(500);
            builder.Property(c => c.BusinessHours).HasMaxLength(200);
        }
    }
}
