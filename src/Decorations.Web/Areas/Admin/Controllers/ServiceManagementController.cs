using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decorations.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceManagementController : Controller
    {
        private readonly IServiceCatalogService serviceCatalogService;
        private readonly ILogger<ServiceManagementController> logger;

        public ServiceManagementController(
            IServiceCatalogService serviceCatalogService,
            ILogger<ServiceManagementController> logger)
        {
            this.serviceCatalogService = serviceCatalogService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IReadOnlyList<ServiceDto> services = await this.serviceCatalogService.GetAllServicesAsync();
            return this.View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new ServiceDto { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceDto dto)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(dto);
            }

            await this.serviceCatalogService.CreateServiceAsync(dto);
            this.logger.LogInformation("ServiceManagementController.Create - Servicio creado: {Title}", dto.Title);
            this.TempData["Success"] = "Servicio creado correctamente.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ServiceDto? service = await this.serviceCatalogService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return this.NotFound();
            }

            return this.View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceDto dto)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(dto);
            }

            await this.serviceCatalogService.UpdateServiceAsync(dto);
            this.logger.LogInformation("ServiceManagementController.Edit - Servicio actualizado: {Id}", dto.Id);
            this.TempData["Success"] = "Servicio actualizado correctamente.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await this.serviceCatalogService.DeleteServiceAsync(id);
            this.logger.LogInformation("ServiceManagementController.Delete - Servicio eliminado: {Id}", id);
            this.TempData["Success"] = "Servicio eliminado correctamente.";
            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
