using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Domain.Entities;

namespace Decorations.Application.Services
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly IRepository<Service> repository;

        public ServiceCatalogService(IRepository<Service> repository)
        {
            this.repository = repository;
        }

        public async Task<IReadOnlyList<ServiceDto>> GetAllActiveServicesAsync()
        {
            IReadOnlyList<Service> services = await this.repository.FindAsync(s => s.IsActive);
            return services.OrderBy(s => s.DisplayOrder)
                           .Select(s => MapToDto(s))
                           .ToList();
        }

        public async Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync()
        {
            IReadOnlyList<Service> services = await this.repository.GetAllAsync();
            return services.OrderBy(s => s.DisplayOrder)
                           .Select(s => MapToDto(s))
                           .ToList();
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(int id)
        {
            Service? service = await this.repository.GetByIdAsync(id);
            return service != null ? MapToDto(service) : null;
        }

        public async Task<ServiceDto> CreateServiceAsync(ServiceDto dto)
        {
            Service service = MapToNewEntity(dto);
            await this.repository.AddAsync(service);
            await this.repository.SaveChangesAsync();
            return MapToDto(service);
        }

        public async Task UpdateServiceAsync(ServiceDto dto)
        {
            Service? service = await this.repository.GetByIdAsync(dto.Id);
            if (service == null)
            {
                return;
            }

            UpdateEntityFromDto(service, dto);
            this.repository.Update(service);
            await this.repository.SaveChangesAsync();
        }

        public async Task DeleteServiceAsync(int id)
        {
            Service? service = await this.repository.GetByIdAsync(id);
            if (service == null)
            {
                return;
            }

            this.repository.Delete(service);
            await this.repository.SaveChangesAsync();
        }

        private static ServiceDto MapToDto(Service service)
        {
            return new ServiceDto
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                IconCssClass = service.IconCssClass,
                IsActive = service.IsActive,
                DisplayOrder = service.DisplayOrder,
                SeoMetaTitle = service.SeoMetaTitle,
                SeoMetaDescription = service.SeoMetaDescription,
                SeoOpenGraphImageUrl = service.SeoOpenGraphImageUrl
            };
        }

        private static Service MapToNewEntity(ServiceDto dto)
        {
            return new Service
            {
                Title = dto.Title,
                Description = dto.Description,
                IconCssClass = dto.IconCssClass,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                SeoMetaTitle = dto.SeoMetaTitle,
                SeoMetaDescription = dto.SeoMetaDescription,
                SeoOpenGraphImageUrl = dto.SeoOpenGraphImageUrl
            };
        }

        private static void UpdateEntityFromDto(Service entity, ServiceDto dto)
        {
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.IconCssClass = dto.IconCssClass;
            entity.IsActive = dto.IsActive;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.SeoMetaTitle = dto.SeoMetaTitle;
            entity.SeoMetaDescription = dto.SeoMetaDescription;
            entity.SeoOpenGraphImageUrl = dto.SeoOpenGraphImageUrl;
        }
    }
}
