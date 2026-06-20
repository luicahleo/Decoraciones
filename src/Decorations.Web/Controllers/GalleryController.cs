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
    }
}
