using Decorations.Application.DTOs;

namespace Decorations.Application.Interfaces
{
    public interface IServiceCatalogService
    {
        Task<IReadOnlyList<ServiceDto>> GetAllActiveServicesAsync();
        Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync();
        Task<ServiceDto?> GetServiceByIdAsync(int id);
        Task<ServiceDto> CreateServiceAsync(ServiceDto dto);
        Task UpdateServiceAsync(ServiceDto dto);
        Task DeleteServiceAsync(int id);
    }
}
