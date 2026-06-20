using Decorations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Decorations.IntegrationTests.Helpers
{
    public static class DatabaseFactory
    {
        public static ApplicationDbContext CreateInMemoryContext()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
