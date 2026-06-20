using Decorations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decorations.Infrastructure.Persistence.Configurations
{
    public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Phone).HasMaxLength(20);
            builder.Property(c => c.EventType).HasMaxLength(100);
            builder.Property(c => c.Message).IsRequired().HasMaxLength(2000);
            builder.Property(c => c.ReceivedAt).IsRequired();
        }
    }
}
