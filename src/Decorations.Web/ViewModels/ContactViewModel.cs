using Decorations.Application.DTOs;

namespace Decorations.Web.ViewModels
{
    public class ContactViewModel : SeoViewModel
    {
        public ContactMessageDto Form { get; set; } = new ContactMessageDto();
        public ContactSettingsDto ContactSettings { get; set; } = new ContactSettingsDto();
    }
}
