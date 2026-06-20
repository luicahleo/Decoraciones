using Decorations.Application.DTOs;

namespace Decorations.Application.Interfaces
{
    public interface IContactService
    {
        Task SaveContactMessageAsync(ContactMessageDto dto);
        Task<IReadOnlyList<ContactMessageDto>> GetAllContactMessagesAsync();
        Task MarkMessageAsReadAsync(int id);
        Task DeleteContactMessageAsync(int id);
    }
}
