using System.Linq.Expressions;
using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Application.Services;
using Decorations.Domain.Entities;
using Moq;

namespace Decorations.UnitTests.Services
{
    public class ServiceCatalogServiceTests
    {
        private readonly Mock<IRepository<Service>> repositoryMock;
        private readonly ServiceCatalogService service;

        public ServiceCatalogServiceTests()
        {
            this.repositoryMock = new Mock<IRepository<Service>>();
            this.service = new ServiceCatalogService(this.repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllActiveServicesAsync_WhenOneActiveServiceExists_ReturnsSingleItem()
        {
            IReadOnlyList<Service> activeServices = new List<Service>
            {
                new Service { Id = 1, Title = "Cumpleaños", IsActive = true, DisplayOrder = 1 }
            };
            this.repositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Service, bool>>>()))
                .ReturnsAsync(activeServices);

            IReadOnlyList<ServiceDto> result = await this.service.GetAllActiveServicesAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllActiveServicesAsync_WhenServiceExists_MapsTitleCorrectly()
        {
            IReadOnlyList<Service> activeServices = new List<Service>
            {
                new Service { Id = 1, Title = "Bautizos", IsActive = true, DisplayOrder = 1 }
            };
            this.repositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Service, bool>>>()))
                .ReturnsAsync(activeServices);

            IReadOnlyList<ServiceDto> result = await this.service.GetAllActiveServicesAsync();

            Assert.Equal("Bautizos", result.First().Title);
        }

        [Fact]
        public async Task GetAllActiveServicesAsync_WhenNoActiveServices_ReturnsEmptyList()
        {
            this.repositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Service, bool>>>()))
                .ReturnsAsync(new List<Service>());

            IReadOnlyList<ServiceDto> result = await this.service.GetAllActiveServicesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllActiveServicesAsync_WhenMultipleServices_ReturnsOrderedByDisplayOrder()
        {
            IReadOnlyList<Service> activeServices = new List<Service>
            {
                new Service { Id = 2, Title = "Segundo", IsActive = true, DisplayOrder = 2 },
                new Service { Id = 1, Title = "Primero", IsActive = true, DisplayOrder = 1 }
            };
            this.repositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Service, bool>>>()))
                .ReturnsAsync(activeServices);

            IReadOnlyList<ServiceDto> result = await this.service.GetAllActiveServicesAsync();

            Assert.Equal("Primero", result.First().Title);
        }

        [Fact]
        public async Task CreateServiceAsync_WithValidDto_ReturnsServiceWithSameTitle()
        {
            ServiceDto dto = new ServiceDto { Title = "Comuniones", Description = "Desc", IsActive = true };
            this.repositoryMock.Setup(r => r.AddAsync(It.IsAny<Service>())).Returns(Task.CompletedTask);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            ServiceDto result = await this.service.CreateServiceAsync(dto);

            Assert.Equal("Comuniones", result.Title);
        }

        [Fact]
        public async Task CreateServiceAsync_WithValidDto_CallsRepositoryAddAsync()
        {
            ServiceDto dto = new ServiceDto { Title = "Bodas", Description = "Desc" };
            this.repositoryMock.Setup(r => r.AddAsync(It.IsAny<Service>())).Returns(Task.CompletedTask);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.CreateServiceAsync(dto);

            this.repositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>()), Times.Once);
        }

        [Fact]
        public async Task CreateServiceAsync_WithValidDto_CallsSaveChangesAsync()
        {
            ServiceDto dto = new ServiceDto { Title = "Bodas", Description = "Desc" };
            this.repositoryMock.Setup(r => r.AddAsync(It.IsAny<Service>())).Returns(Task.CompletedTask);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.CreateServiceAsync(dto);

            this.repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteServiceAsync_WhenServiceNotFound_DoesNotCallDelete()
        {
            this.repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Service?)null);

            await this.service.DeleteServiceAsync(99);

            this.repositoryMock.Verify(r => r.Delete(It.IsAny<Service>()), Times.Never);
        }

        [Fact]
        public async Task DeleteServiceAsync_WhenServiceExists_CallsRepositoryDelete()
        {
            Service existingService = new Service { Id = 1, Title = "A eliminar" };
            this.repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingService);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.DeleteServiceAsync(1);

            this.repositoryMock.Verify(r => r.Delete(existingService), Times.Once);
        }

        [Fact]
        public async Task GetServiceByIdAsync_WhenNotFound_ReturnsNull()
        {
            this.repositoryMock.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((Service?)null);

            ServiceDto? result = await this.service.GetServiceByIdAsync(404);

            Assert.Null(result);
        }
    }
}
