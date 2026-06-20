using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Application.Services;
using Decorations.Domain.Entities;
using Moq;

namespace Decorations.UnitTests.Services
{
    public class ContactSettingsServiceTests
    {
        private readonly Mock<IRepository<ContactSettings>> repositoryMock;
        private readonly ContactSettingsService service;

        public ContactSettingsServiceTests()
        {
            this.repositoryMock = new Mock<IRepository<ContactSettings>>();
            this.service = new ContactSettingsService(this.repositoryMock.Object);
        }

        [Fact]
        public async Task GetContactSettingsAsync_WhenSettingsExist_ReturnsMappedBusinessName()
        {
            IReadOnlyList<ContactSettings> settings = new List<ContactSettings>
            {
                new ContactSettings { Id = 1, BusinessName = "Fiestas Bonitas", WhatsAppNumber = "+34600000001" }
            };
            this.repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(settings);

            ContactSettingsDto result = await this.service.GetContactSettingsAsync();

            Assert.Equal("Fiestas Bonitas", result.BusinessName);
        }

        [Fact]
        public async Task GetContactSettingsAsync_WhenNoSettingsExist_ReturnsEmptyBusinessName()
        {
            this.repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ContactSettings>());

            ContactSettingsDto result = await this.service.GetContactSettingsAsync();

            Assert.Equal(string.Empty, result.BusinessName);
        }

        [Fact]
        public async Task GetContactSettingsAsync_WhenSettingsExist_ReturnsWhatsAppNumber()
        {
            IReadOnlyList<ContactSettings> settings = new List<ContactSettings>
            {
                new ContactSettings { Id = 1, BusinessName = "Test", WhatsAppNumber = "+34611222333" }
            };
            this.repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(settings);

            ContactSettingsDto result = await this.service.GetContactSettingsAsync();

            Assert.Equal("+34611222333", result.WhatsAppNumber);
        }

        [Fact]
        public async Task UpdateContactSettingsAsync_WhenSettingsFound_UpdatesBusinessName()
        {
            ContactSettings existingSettings = new ContactSettings { Id = 1, BusinessName = "Antiguo Nombre" };
            this.repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingSettings);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            ContactSettingsDto dto = new ContactSettingsDto { Id = 1, BusinessName = "Nuevo Nombre", WhatsAppNumber = "+34600000000" };

            await this.service.UpdateContactSettingsAsync(dto);

            Assert.Equal("Nuevo Nombre", existingSettings.BusinessName);
        }

        [Fact]
        public async Task UpdateContactSettingsAsync_WhenSettingsNotFound_DoesNotCallUpdate()
        {
            this.repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ContactSettings?)null);
            ContactSettingsDto dto = new ContactSettingsDto { Id = 999, BusinessName = "Test" };

            await this.service.UpdateContactSettingsAsync(dto);

            this.repositoryMock.Verify(r => r.Update(It.IsAny<ContactSettings>()), Times.Never);
        }

        [Fact]
        public async Task UpdateContactSettingsAsync_WhenSettingsFound_CallsSaveChanges()
        {
            ContactSettings existingSettings = new ContactSettings { Id = 1, BusinessName = "Original" };
            this.repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingSettings);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            ContactSettingsDto dto = new ContactSettingsDto { Id = 1, BusinessName = "Actualizado" };

            await this.service.UpdateContactSettingsAsync(dto);

            this.repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
