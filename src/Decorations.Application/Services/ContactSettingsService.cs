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
            // El model binding convierte los campos vacíos del formulario en null;
            // las columnas de la BD son NOT NULL, así que coalescemos a cadena vacía.
            entity.BusinessName = dto.BusinessName ?? string.Empty;
            entity.WhatsAppNumber = dto.WhatsAppNumber ?? string.Empty;
            entity.Email = dto.Email ?? string.Empty;
            entity.InstagramUrl = dto.InstagramUrl ?? string.Empty;
            entity.FacebookUrl = dto.FacebookUrl ?? string.Empty;
            entity.Address = dto.Address ?? string.Empty;
            entity.BusinessHours = dto.BusinessHours ?? string.Empty;
        }
    }
}
