using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Decorations.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GalleryManagementController : Controller
    {
        private readonly IGalleryService galleryService;
        private readonly ILogger<GalleryManagementController> logger;

        public GalleryManagementController(IGalleryService galleryService, ILogger<GalleryManagementController> logger)
        {
            this.galleryService = galleryService;
            this.logger = logger;
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
            this.logger.LogDebug("GalleryManagementController.Create - Iniciando creación. ModelState válido: {IsValid}", this.ModelState.IsValid);
            this.logger.LogDebug("GalleryManagementController.Create - Título recibido: '{Title}'", viewModel?.Item?.Title);
            this.logger.LogDebug("GalleryManagementController.Create - Archivo recibido: {FileName} ({FileSize} bytes)", 
                imageFile?.FileName, imageFile?.Length);

            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("GalleryManagementController.Create - ModelState inválido. Errores: {Errors}", 
                    string.Join(", ", this.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return this.View(viewModel);
            }

            GalleryItemDto createdItem = await this.galleryService.CreateGalleryItemAsync(viewModel.Item);
            this.logger.LogInformation("GalleryManagementController.Create - Elemento creado con ID: {ItemId}", createdItem.Id);

            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    using Stream stream = imageFile.OpenReadStream();
                    MediaAssetDto asset = await this.galleryService.AddImageToGalleryItemAsync(
                        createdItem.Id,
                        stream,
                        imageFile.FileName,
                        viewModel.Item.Title);
                    this.logger.LogInformation("GalleryManagementController.Create - Imagen guardada: {FilePath}", asset.FilePath);
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, "GalleryManagementController.Create - Error al guardar imagen");
                }
            }

            if (!string.IsNullOrWhiteSpace(viewModel.YoutubeVideoId))
            {
                await this.galleryService.AddVideoToGalleryItemAsync(
                    createdItem.Id,
                    viewModel.YoutubeVideoId,
                    viewModel.VideoAltText);
            }

            this.logger.LogInformation("GalleryManagementController.Create - Redirigiendo a Index");
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
