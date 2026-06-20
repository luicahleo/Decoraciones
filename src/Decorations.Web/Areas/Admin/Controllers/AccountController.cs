using Decorations.Infrastructure.Identity;
using Decorations.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Decorations.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "")
        {
            LoginViewModel viewModel = new LoginViewModel { ReturnUrl = returnUrl };
            return this.View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            IdentitySignInResult result = await this.signInManager.PasswordSignInAsync(
                viewModel.Email,
                viewModel.Password,
                viewModel.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(viewModel.ReturnUrl) && this.Url.IsLocalUrl(viewModel.ReturnUrl))
                {
                    return this.Redirect(viewModel.ReturnUrl);
                }

                return this.RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            if (result.IsLockedOut)
            {
                this.ModelState.AddModelError(string.Empty, "Cuenta bloqueada por exceso de intentos. Intente más tarde.");
                return this.View(viewModel);
            }

            this.ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return this.RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
