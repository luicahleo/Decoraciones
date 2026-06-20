using Decorations.Domain.Entities;
using Decorations.Infrastructure.Identity;
using Decorations.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Decorations.Infrastructure.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            await SeedAdminRoleAsync(roleManager);
            await SeedAdminUserAsync(userManager, configuration);
            await SeedDefaultContactSettingsAsync(context);
        }

        private static async Task SeedAdminRoleAsync(RoleManager<IdentityRole> roleManager)
        {
            bool adminRoleExists = await roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            string adminEmail = configuration["SeedSettings:AdminEmail"] ?? "admin@decoraciones.com";
            string adminPassword = configuration["SeedSettings:AdminPassword"] ?? string.Empty;
            string adminFullName = configuration["SeedSettings:AdminFullName"] ?? "Administrador";

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            ApplicationUser? existingUser = await userManager.FindByEmailAsync(adminEmail);
            if (existingUser != null)
            {
                return;
            }

            ApplicationUser adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                EmailConfirmed = true
            };

            IdentityResult createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        private static async Task SeedDefaultContactSettingsAsync(ApplicationDbContext context)
        {
            bool settingsExist = await context.ContactSettings.AnyAsync();
            if (!settingsExist)
            {
                ContactSettings defaultSettings = new ContactSettings
                {
                    BusinessName = "Decoraciones Especiales",
                    WhatsAppNumber = "+34600000000",
                    Email = "info@decoraciones.com",
                    InstagramUrl = string.Empty,
                    FacebookUrl = string.Empty,
                    Address = string.Empty,
                    BusinessHours = "Lunes a Viernes: 9:00 - 18:00"
                };

                context.ContactSettings.Add(defaultSettings);
                await context.SaveChangesAsync();
            }
        }
    }
}
