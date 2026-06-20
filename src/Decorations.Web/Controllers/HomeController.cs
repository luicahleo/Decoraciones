using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Decorations.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceCatalogService serviceCatalogService;
        private readonly IGalleryService galleryService;
        private readonly IContactSettingsService contactSettingsService;

        public HomeController(
            IServiceCatalogService serviceCatalogService,
            IGalleryService galleryService,
            IContactSettingsService contactSettingsService)
        {
            this.serviceCatalogService = serviceCatalogService;
            this.galleryService = galleryService;
            this.contactSettingsService = contactSettingsService;
        }

        public async Task<IActionResult> Index()
        {
            IReadOnlyList<ServiceDto> services = await this.serviceCatalogService.GetAllActiveServicesAsync();
            IReadOnlyList<GalleryItemDto> galleryItems = await this.galleryService.GetAllActiveGalleryItemsAsync();
            ContactSettingsDto contactSettings = await this.contactSettingsService.GetContactSettingsAsync();

            HomeViewModel viewModel = new HomeViewModel
            {
                Services = services,
                GalleryPreview = galleryItems.Take(6).ToList(),
                ContactSettings = contactSettings,
                PageTitle = $"{contactSettings.BusinessName} - Decoraciones para Eventos",
                MetaDescription = "Creamos experiencias magicas para tus eventos especiales.",
                OgTitle = $"{contactSettings.BusinessName} - Decoraciones para Eventos",
                OgDescription = "Creamos experiencias magicas para tus eventos especiales.",
                OgImage = string.Empty
            };

            return this.View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View();
        }
    }
}
