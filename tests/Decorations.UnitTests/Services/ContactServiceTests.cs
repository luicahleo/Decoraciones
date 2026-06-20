using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Application.Services;
using Decorations.Domain.Entities;
using Moq;

namespace Decorations.UnitTests.Services
{
    public class ContactServiceTests
    {
        private readonly Mock<IRepository<ContactMessage>> repositoryMock;
        private readonly ContactService service;

        public ContactServiceTests()
        {
            this.repositoryMock = new Mock<IRepository<ContactMessage>>();
            this.service = new ContactService(this.repositoryMock.Object);
        }

        [Fact]
        public async Task SaveContactMessageAsync_SetsIsReadToFalse()
        {
            ContactMessage? capturedMessage = null;
            this.repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ContactMessage>()))
                .Callback<ContactMessage>(m => capturedMessage = m)
                .Returns(Task.CompletedTask);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            ContactMessageDto dto = new ContactMessageDto { Name = "Ana", Email = "ana@test.com", Message = "Consulta" };

            await this.service.SaveContactMessageAsync(dto);

            Assert.False(capturedMessage!.IsRead);
        }

        [Fact]
        public async Task SaveContactMessageAsync_MapsNameFromDto()
        {
            ContactMessage? capturedMessage = null;
            this.repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ContactMessage>()))
                .Callback<ContactMessage>(m => capturedMessage = m)
                .Returns(Task.CompletedTask);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            ContactMessageDto dto = new ContactMessageDto { Name = "María López", Email = "maria@test.com", Message = "Consulta" };

            await this.service.SaveContactMessageAsync(dto);

            Assert.Equal("María López", capturedMessage!.Name);
        }

        [Fact]
        public async Task SaveContactMessageAsync_CallsSaveChangesAsync()
        {
            this.repositoryMock.Setup(r => r.AddAsync(It.IsAny<ContactMessage>())).Returns(Task.CompletedTask);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            ContactMessageDto dto = new ContactMessageDto { Name = "Pedro", Email = "pedro@test.com", Message = "Hola" };

            await this.service.SaveContactMessageAsync(dto);

            this.repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WhenMessageExists_SetsIsReadToTrue()
        {
            ContactMessage existingMessage = new ContactMessage { Id = 1, IsRead = false, Name = "Luis", Email = "luis@test.com", Message = "Test" };
            this.repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingMessage);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.MarkMessageAsReadAsync(1);

            Assert.True(existingMessage.IsRead);
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WhenMessageNotFound_DoesNotCallUpdate()
        {
            this.repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ContactMessage?)null);

            await this.service.MarkMessageAsReadAsync(999);

            this.repositoryMock.Verify(r => r.Update(It.IsAny<ContactMessage>()), Times.Never);
        }

        [Fact]
        public async Task DeleteContactMessageAsync_WhenMessageExists_CallsRepositoryDelete()
        {
            ContactMessage existingMessage = new ContactMessage { Id = 5, Name = "Rosa", Email = "rosa@test.com", Message = "Test" };
            this.repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existingMessage);
            this.repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await this.service.DeleteContactMessageAsync(5);

            this.repositoryMock.Verify(r => r.Delete(existingMessage), Times.Once);
        }

        [Fact]
        public async Task DeleteContactMessageAsync_WhenMessageNotFound_DoesNotCallDelete()
        {
            this.repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ContactMessage?)null);

            await this.service.DeleteContactMessageAsync(999);

            this.repositoryMock.Verify(r => r.Delete(It.IsAny<ContactMessage>()), Times.Never);
        }

        [Fact]
        public async Task GetAllContactMessagesAsync_WhenMessagesExist_ReturnsMappedDtos()
        {
            IReadOnlyList<ContactMessage> messages = new List<ContactMessage>
            {
                new ContactMessage { Id = 1, Name = "Carlos", Email = "carlos@test.com", Message = "Hola", ReceivedAt = DateTime.UtcNow },
                new ContactMessage { Id = 2, Name = "Laura", Email = "laura@test.com", Message = "Test", ReceivedAt = DateTime.UtcNow }
            };
            this.repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(messages);

            IReadOnlyList<ContactMessageDto> result = await this.service.GetAllContactMessagesAsync();

            Assert.Equal(2, result.Count);
        }
    }
}
