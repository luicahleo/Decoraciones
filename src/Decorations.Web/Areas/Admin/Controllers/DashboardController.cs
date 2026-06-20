using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decorations.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IGalleryService galleryService;
        private readonly IContactService contactService;

        public DashboardController(IGalleryService galleryService, IContactService contactService)
        {
            this.galleryService = galleryService;
            this.contactService = contactService;
        }

        public async Task<IActionResult> Index()
        {
            IReadOnlyList<GalleryItemDto> galleryItems = await this.galleryService.GetAllGalleryItemsAsync();
            IReadOnlyList<ContactMessageDto> messages = await this.contactService.GetAllContactMessagesAsync();

            GalleryManagementViewModel viewModel = new GalleryManagementViewModel
            {
                GalleryItems = galleryItems,
                UnreadMessagesCount = messages.Count(m => !m.IsRead)
            };

            return this.View(viewModel);
        }
    }
}
