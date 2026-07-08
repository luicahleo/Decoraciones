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
        public async Task<IActionResult> Create(GalleryItemFormViewModel viewModel, IFormFileCollection? imageFiles)
        {
            this.logger.LogDebug("GalleryManagementController.Create - Iniciando creación. ModelState válido: {IsValid}", this.ModelState.IsValid);
            this.logger.LogDebug("GalleryManagementController.Create - Título recibido: '{Title}'", viewModel?.Item?.Title);
            this.logger.LogDebug("GalleryManagementController.Create - ShowAsGrid: {ShowAsGrid}", viewModel?.Item?.ShowAsGrid);
            this.logger.LogDebug("GalleryManagementController.Create - Archivos recibidos: {FileCount}", imageFiles?.Count ?? 0);

            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("GalleryManagementController.Create - ModelState inválido. Errores: {Errors}", 
                    string.Join(", ", this.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return this.View(viewModel);
            }

            // Crear elemento de galería
            GalleryItemDto createdItem = await this.galleryService.CreateGalleryItemAsync(viewModel.Item);
            this.logger.LogInformation("GalleryManagementController.Create - Elemento creado con ID: {ItemId}", createdItem.Id);

            // Procesar múltiples imágenes
            if (imageFiles != null && imageFiles.Count > 0)
            {
                string? featuredImageIndexStr = this.HttpContext.Request.Form["featuredImageIndex"];
                int featuredImageIndex = -1;
                int.TryParse(featuredImageIndexStr, out featuredImageIndex);

                for (int index = 0; index < imageFiles.Count; index++)
                {
                    IFormFile imageFile = imageFiles[index];
                    
                    if (imageFile.Length == 0) continue;

                    try
                    {
                        using Stream stream = imageFile.OpenReadStream();
                        MediaAssetDto asset = await this.galleryService.AddImageToGalleryItemAsync(
                            createdItem.Id,
                            stream,
                            imageFile.FileName,
                            viewModel.Item.Title);
                        
                        // Marcar como portada si es la imagen seleccionada
                        if (index == featuredImageIndex)
                        {
                            asset.IsFeatured = true;
                            await this.galleryService.UpdateMediaAssetAsync(asset);
                            this.logger.LogInformation("GalleryManagementController.Create - Imagen marcada como portada: {FilePath}", asset.FullSizePath);
                        }

                        this.logger.LogInformation("GalleryManagementController.Create - Imagen guardada: {FilePath}", asset.FullSizePath);
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogError(exception, "GalleryManagementController.Create - Error al guardar imagen: {FileName}", imageFile.FileName);
                    }
                }
            }

            // Agregar vídeo si se proporciona
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
        public async Task<IActionResult> Edit(GalleryItemFormViewModel viewModel, IFormFileCollection? imageFiles)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            await this.galleryService.UpdateGalleryItemAsync(viewModel.Item);

            // Procesar múltiples imágenes nuevas
            if (imageFiles != null && imageFiles.Count > 0)
            {
                for (int index = 0; index < imageFiles.Count; index++)
                {
                    IFormFile imageFile = imageFiles[index];
                    
                    if (imageFile.Length == 0) continue;

                    try
                    {
                        using Stream stream = imageFile.OpenReadStream();
                        await this.galleryService.AddImageToGalleryItemAsync(
                            viewModel.Item.Id,
                            stream,
                            imageFile.FileName,
                            viewModel.Item.Title);
                        
                        this.logger.LogInformation("GalleryManagementController.Edit - Imagen guardada: {FileName}", imageFile.FileName);
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogError(exception, "GalleryManagementController.Edit - Error al guardar imagen: {FileName}", imageFile.FileName);
                    }
                }
            }

            // Agregar vídeo si se proporciona
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int[] orderedIds)
        {
            await this.galleryService.ReorderGalleryItemsAsync(orderedIds);
            return this.Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int mediaAssetId, int galleryItemId)
        {
            await this.galleryService.SetFeaturedMediaAssetAsync(galleryItemId, mediaAssetId);
            return this.RedirectToAction(nameof(this.Edit), new { id = galleryItemId });
        }
    }
}
