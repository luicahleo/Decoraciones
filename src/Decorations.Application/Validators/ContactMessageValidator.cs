using Decorations.Application.DTOs;
using FluentValidation;

namespace Decorations.Application.Validators
{
    public class ContactMessageValidator : AbstractValidator<ContactMessageDto>
    {
        public ContactMessageValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(200);

            RuleFor(x => x.Phone)
                .MaximumLength(20);

            RuleFor(x => x.EventType)
                .MaximumLength(100);

            RuleFor(x => x.Message)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }
}
