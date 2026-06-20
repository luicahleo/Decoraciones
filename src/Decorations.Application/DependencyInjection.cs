using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Application.Services;
using Decorations.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Decorations.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
            services.AddScoped<IGalleryService, GalleryService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IContactSettingsService, ContactSettingsService>();
            services.AddScoped<IValidator<ContactMessageDto>, ContactMessageValidator>();
            return services;
        }
    }
}
