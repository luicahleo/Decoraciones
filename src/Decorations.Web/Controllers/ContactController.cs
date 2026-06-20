using Decorations.Application.DTOs;
using Decorations.Application.Interfaces;
using Decorations.Web.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Decorations.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactService contactService;
        private readonly IContactSettingsService contactSettingsService;
        private readonly IValidator<ContactMessageDto> validator;

        public ContactController(
            IContactService contactService,
            IContactSettingsService contactSettingsService,
            IValidator<ContactMessageDto> validator)
        {
            this.contactService = contactService;
            this.contactSettingsService = contactSettingsService;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ContactSettingsDto settings = await this.contactSettingsService.GetContactSettingsAsync();

            ContactViewModel viewModel = new ContactViewModel
            {
                Form = new ContactMessageDto(),
                ContactSettings = settings,
                PageTitle = "Contacto - Solicitar Presupuesto",
                MetaDescription = "Contacta con nosotros para solicitar presupuesto para tu evento especial."
            };

            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel viewModel)
        {
            ValidationResult validationResult = await this.validator.ValidateAsync(viewModel.Form);

            if (!validationResult.IsValid)
            {
                validationResult.Errors.ForEach(e =>
                    this.ModelState.AddModelError(
                        $"Form.{e.PropertyName}",
                        e.ErrorMessage));

                ContactSettingsDto settings = await this.contactSettingsService.GetContactSettingsAsync();
                viewModel.ContactSettings = settings;
                return this.View(viewModel);
            }

            await this.contactService.SaveContactMessageAsync(viewModel.Form);
            return this.RedirectToAction(nameof(this.Confirmation));
        }

        [HttpGet]
        public IActionResult Confirmation()
        {
            return this.View();
        }
    }
}
