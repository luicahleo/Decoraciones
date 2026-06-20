using System.ComponentModel.DataAnnotations;

namespace Decorations.Web.ViewModels.Admin
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
