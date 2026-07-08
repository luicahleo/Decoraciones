using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Decorations.Web.Controllers
{
    public class GalleryController : Controller
    {
        private readonly IGalleryService galleryService;

        public GalleryController(IGalleryService galleryService)
        {
            this.galleryService = galleryService;
        }

        public async Task<IActionResult> Index()
        {
            IReadOnlyList<GalleryItemDto> items = await this.galleryService.GetAllActiveGalleryItemsAsync();

            GalleryViewModel viewModel = new GalleryViewModel
            {
                GalleryItems = items,
                PageTitle = "Galería - Nuestros Trabajos",
                MetaDescription = "Explora nuestra galería de decoraciones para eventos especiales: cumpleaños, bautizos y más."
            };

            return this.View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            GalleryItemDto? item = await this.galleryService.GetGalleryItemByIdAsync(id);
            if (item == null || !item.IsActive)
            {
                return this.NotFound();
            }

            GalleryDetailsViewModel viewModel = new GalleryDetailsViewModel
            {
                Item = item,
                PageTitle = $"{item.Title} - Nuestros Trabajos",
                MetaDescription = string.IsNullOrWhiteSpace(item.Description)
                    ? "Detalle de nuestra decoración para eventos especiales."
                    : item.Description
            };

            return this.View(viewModel);
        }
    }
}
