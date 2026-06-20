using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Domain.Entities;

namespace Decorations.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IRepository<ContactMessage> repository;

        public ContactService(IRepository<ContactMessage> repository)
        {
            this.repository = repository;
        }

        public async Task SaveContactMessageAsync(ContactMessageDto dto)
        {
            ContactMessage message = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                EventType = dto.EventType,
                Message = dto.Message,
                ReceivedAt = DateTime.UtcNow,
                IsRead = false
            };

            await this.repository.AddAsync(message);
            await this.repository.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<ContactMessageDto>> GetAllContactMessagesAsync()
        {
            IReadOnlyList<ContactMessage> messages = await this.repository.GetAllAsync();
            return messages.OrderByDescending(m => m.ReceivedAt)
                           .Select(m => MapToDto(m))
                           .ToList();
        }

        public async Task MarkMessageAsReadAsync(int id)
        {
            ContactMessage? message = await this.repository.GetByIdAsync(id);
            if (message == null)
            {
                return;
            }

            message.IsRead = true;
            this.repository.Update(message);
            await this.repository.SaveChangesAsync();
        }

        public async Task DeleteContactMessageAsync(int id)
        {
            ContactMessage? message = await this.repository.GetByIdAsync(id);
            if (message == null)
            {
                return;
            }

            this.repository.Delete(message);
            await this.repository.SaveChangesAsync();
        }

        private static ContactMessageDto MapToDto(ContactMessage message)
        {
            return new ContactMessageDto
            {
                Id = message.Id,
                Name = message.Name,
                Email = message.Email,
                Phone = message.Phone,
                EventType = message.EventType,
                Message = message.Message,
                ReceivedAt = message.ReceivedAt,
                IsRead = message.IsRead
            };
        }
    }
}
