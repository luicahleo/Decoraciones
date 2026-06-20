using Decorations.Domain.Entities;
using Decorations.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Decorations.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<GalleryItem> GalleryItems { get; set; } = null!;
        public DbSet<MediaAsset> MediaAssets { get; set; } = null!;
        public DbSet<ContactSettings> ContactSettings { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
