using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decorations.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactSettingsController : Controller
    {
        private readonly IContactSettingsService contactSettingsService;
        private readonly IContactService contactService;

        public ContactSettingsController(
            IContactSettingsService contactSettingsService,
            IContactService contactService)
        {
            this.contactSettingsService = contactSettingsService;
            this.contactService = contactService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ContactSettingsDto settings = await this.contactSettingsService.GetContactSettingsAsync();
            return this.View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactSettingsDto dto)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(dto);
            }

            await this.contactSettingsService.UpdateContactSettingsAsync(dto);
            this.TempData["Success"] = "Configuración guardada correctamente.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public async Task<IActionResult> Messages()
        {
            IReadOnlyList<ContactMessageDto> messages = await this.contactService.GetAllContactMessagesAsync();
            return this.View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await this.contactService.MarkMessageAsReadAsync(id);
            return this.RedirectToAction(nameof(this.Messages));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            await this.contactService.DeleteContactMessageAsync(id);
            return this.RedirectToAction(nameof(this.Messages));
        }
    }
}
