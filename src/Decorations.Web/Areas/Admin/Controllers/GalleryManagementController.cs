using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decorations.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GalleryManagementController : Controller
    {
        private readonly IGalleryService galleryService;

        public GalleryManagementController(IGalleryService galleryService)
        {
            this.galleryService = galleryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IReadOnlyList<GalleryItemDto> items = await this.galleryService.GetAllGalleryItemsAsync();
            GalleryManagementViewModel viewModel = new GalleryManagementViewModel { GalleryItems = items };
            return this.View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new GalleryItemFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GalleryItemFormViewModel viewModel, IFormFile? imageFile)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            GalleryItemDto createdItem = await this.galleryService.CreateGalleryItemAsync(viewModel.Item);

            if (imageFile != null && imageFile.Length > 0)
            {
                using Stream stream = imageFile.OpenReadStream();
                await this.galleryService.AddImageToGalleryItemAsync(
                    createdItem.Id,
                    stream,
                    imageFile.FileName,
                    viewModel.Item.Title);
            }

            if (!string.IsNullOrWhiteSpace(viewModel.YoutubeVideoId))
            {
                await this.galleryService.AddVideoToGalleryItemAsync(
                    createdItem.Id,
                    viewModel.YoutubeVideoId,
                    viewModel.VideoAltText);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            GalleryItemDto? item = await this.galleryService.GetGalleryItemByIdAsync(id);
            if (item == null)
            {
                return this.NotFound();
            }

            GalleryItemFormViewModel viewModel = new GalleryItemFormViewModel { Item = item };
            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GalleryItemFormViewModel viewModel, IFormFile? imageFile)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            await this.galleryService.UpdateGalleryItemAsync(viewModel.Item);

            if (imageFile != null && imageFile.Length > 0)
            {
                using Stream stream = imageFile.OpenReadStream();
                await this.galleryService.AddImageToGalleryItemAsync(
                    viewModel.Item.Id,
                    stream,
                    imageFile.FileName,
                    viewModel.Item.Title);
            }

            if (!string.IsNullOrWhiteSpace(viewModel.YoutubeVideoId))
            {
                await this.galleryService.AddVideoToGalleryItemAsync(
                    viewModel.Item.Id,
                    viewModel.YoutubeVideoId,
                    viewModel.VideoAltText);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await this.galleryService.DeleteGalleryItemAsync(id);
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedia(int mediaAssetId, int galleryItemId)
        {
            await this.galleryService.DeleteMediaAssetAsync(mediaAssetId);
            return this.RedirectToAction(nameof(this.Edit), new { id = galleryItemId });
        }
    }
}
