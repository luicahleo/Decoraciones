using Decorations.Application.DTOs;
using Decorations.Application.Validators;
using FluentValidation.Results;

namespace Decorations.UnitTests.Validators
{
    public class ContactMessageValidatorTests
    {
        private readonly ContactMessageValidator validator;

        public ContactMessageValidatorTests()
        {
            this.validator = new ContactMessageValidator();
        }

        [Fact]
        public async Task Validate_WhenNameIsEmpty_IsNotValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = string.Empty, Email = "test@test.com", Message = "Mensaje de prueba" };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_WhenEmailIsEmpty_IsNotValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = "Juan", Email = string.Empty, Message = "Mensaje de prueba" };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_WhenEmailHasInvalidFormat_IsNotValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = "Juan", Email = "no-es-un-email", Message = "Mensaje de prueba" };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_WhenMessageIsEmpty_IsNotValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = "Juan", Email = "juan@test.com", Message = string.Empty };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_WhenNameExceedsMaxLength_IsNotValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = new string('A', 101), Email = "juan@test.com", Message = "Mensaje válido" };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_WhenMessageExceedsMaxLength_IsNotValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = "Juan", Email = "juan@test.com", Message = new string('A', 2001) };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_WhenAllRequiredFieldsAreValid_IsValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = "Juan García", Email = "juan@test.com", Message = "Quiero información sobre decoraciones" };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Validate_WhenPhoneIsEmpty_IsValid()
        {
            ContactMessageDto dto = new ContactMessageDto { Name = "Juan", Email = "juan@test.com", Phone = string.Empty, Message = "Mensaje válido" };

            ValidationResult result = await this.validator.ValidateAsync(dto);

            Assert.True(result.IsValid);
        }
    }
}
