using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Domain.Entities;

namespace Decorations.Application.Services
{
    public class ContactSettingsService : IContactSettingsService
    {
        private readonly IRepository<ContactSettings> repository;

        public ContactSettingsService(IRepository<ContactSettings> repository)
        {
            this.repository = repository;
        }

        public async Task<ContactSettingsDto> GetContactSettingsAsync()
        {
            IReadOnlyList<ContactSettings> allSettings = await this.repository.GetAllAsync();
            ContactSettings settings = allSettings.FirstOrDefault() ?? new ContactSettings();
            return MapToDto(settings);
        }

        public async Task UpdateContactSettingsAsync(ContactSettingsDto dto)
        {
            IReadOnlyList<ContactSettings> allSettings = await this.repository.GetAllAsync();
            ContactSettings? settings = allSettings.FirstOrDefault();
            if (settings == null)
            {
                return;
            }

            UpdateEntityFromDto(settings, dto);
            this.repository.Update(settings);
            await this.repository.SaveChangesAsync();
        }

        private static ContactSettingsDto MapToDto(ContactSettings settings)
        {
            return new ContactSettingsDto
            {
                Id = settings.Id,
                BusinessName = settings.BusinessName,
                WhatsAppNumber = settings.WhatsAppNumber,
                Email = settings.Email,
                InstagramUrl = settings.InstagramUrl,
                FacebookUrl = settings.FacebookUrl,
                Address = settings.Address,
                BusinessHours = settings.BusinessHours
            };
        }

        private static void UpdateEntityFromDto(ContactSettings entity, ContactSettingsDto dto)
        {
            entity.BusinessName = dto.BusinessName;
            entity.WhatsAppNumber = dto.WhatsAppNumber;
            entity.Email = dto.Email;
            entity.InstagramUrl = dto.InstagramUrl;
            entity.FacebookUrl = dto.FacebookUrl;
            entity.Address = dto.Address;
            entity.BusinessHours = dto.BusinessHours;
        }
    }
}
