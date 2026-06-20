using Decorations.Domain.Entities;
using Decorations.Infrastructure.Persistence;
using Decorations.Infrastructure.Repositories;
using Decorations.IntegrationTests.Helpers;

namespace Decorations.IntegrationTests.Repositories
{
    public class RepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);
            await context.Services.AddAsync(new Service { Title = "Test Service", Description = "Desc" });
            await context.SaveChangesAsync();
            Service? savedService = context.Services.First();

            Service? result = await repository.GetByIdAsync(savedService.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenEntityExists_ReturnsTitleCorrectly()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);
            await context.Services.AddAsync(new Service { Title = "Servicio de Prueba", Description = "Desc" });
            await context.SaveChangesAsync();
            Service? savedService = context.Services.First();

            Service? result = await repository.GetByIdAsync(savedService.Id);

            Assert.Equal("Servicio de Prueba", result!.Title);
        }

        [Fact]
        public async Task GetByIdAsync_WhenEntityNotFound_ReturnsNull()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);

            Service? result = await repository.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);
            await context.Services.AddRangeAsync(
                new Service { Title = "Servicio 1", Description = "Desc" },
                new Service { Title = "Servicio 2", Description = "Desc" });
            await context.SaveChangesAsync();

            IReadOnlyList<Service> result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddAsync_AfterSaveChanges_EntityCanBeRetrieved()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);
            Service newService = new Service { Title = "Nuevo Servicio", Description = "Desc" };

            await repository.AddAsync(newService);
            await repository.SaveChangesAsync();
            Service? result = await repository.GetByIdAsync(newService.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task FindAsync_WithPredicate_ReturnsMatchingEntities()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);
            await context.Services.AddRangeAsync(
                new Service { Title = "Activo", Description = "Desc", IsActive = true },
                new Service { Title = "Inactivo", Description = "Desc", IsActive = false });
            await context.SaveChangesAsync();

            IReadOnlyList<Service> result = await repository.FindAsync(s => s.IsActive);

            Assert.Single(result);
        }

        [Fact]
        public async Task Update_WhenEntityUpdated_TitleChangePersists()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);
            Service service = new Service { Title = "Original", Description = "Desc" };
            await context.Services.AddAsync(service);
            await context.SaveChangesAsync();

            service.Title = "Actualizado";
            repository.Update(service);
            await repository.SaveChangesAsync();
            Service? result = await repository.GetByIdAsync(service.Id);

            Assert.Equal("Actualizado", result!.Title);
        }

        [Fact]
        public async Task Delete_WhenEntityDeleted_EntityIsRemovedFromDatabase()
        {
            using ApplicationDbContext context = DatabaseFactory.CreateInMemoryContext();
            Repository<Service> repository = new Repository<Service>(context);
            Service service = new Service { Title = "A Eliminar", Description = "Desc" };
            await context.Services.AddAsync(service);
            await context.SaveChangesAsync();

            repository.Delete(service);
            await repository.SaveChangesAsync();
            Service? result = await repository.GetByIdAsync(service.Id);

            Assert.Null(result);
        }
    }
}
