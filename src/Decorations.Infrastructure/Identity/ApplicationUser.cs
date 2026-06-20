using Microsoft.AspNetCore.Identity;

namespace Decorations.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
