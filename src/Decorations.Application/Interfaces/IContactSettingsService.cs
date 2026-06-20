using Decorations.Application.DTOs;

namespace Decorations.Application.Interfaces
{
    public interface IContactSettingsService
    {
        Task<ContactSettingsDto> GetContactSettingsAsync();
        Task UpdateContactSettingsAsync(ContactSettingsDto dto);
    }
}
