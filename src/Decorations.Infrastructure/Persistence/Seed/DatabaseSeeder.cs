using Decorations.Domain.Entities;
using Decorations.Infrastructure.Identity;
using Decorations.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseSeeder));

            await context.Database.MigrateAsync();

            await SeedAdminRoleAsync(roleManager);
            await SeedAdminUserAsync(userManager, configuration, logger);
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

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
        {
            string adminEmail = configuration["SeedSettings:AdminEmail"] ?? "admin@decoraciones.com";
            string adminPassword = configuration["SeedSettings:AdminPassword"] ?? string.Empty;
            string adminFullName = configuration["SeedSettings:AdminFullName"] ?? "Administrador";

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("Seed de admin omitido: SeedSettings:AdminPassword está vacío. No se creará ningún administrador.");
                return;
            }

            ApplicationUser? existingUser = await userManager.FindByEmailAsync(adminEmail);
            if (existingUser != null)
            {
                logger.LogDebug("Seed de admin omitido: el usuario '{AdminEmail}' ya existe.", adminEmail);
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
            if (!createResult.Succeeded)
            {
                string errores = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                logger.LogError(
                    "No se pudo crear el usuario admin '{AdminEmail}': {Errores}. " +
                    "Revisa que SeedSettings:AdminPassword cumpla la política (>=8, mayúscula, minúscula, dígito y símbolo). " +
                    "La app arranca sin admin; corrige la contraseña y reinicia para reintentar.",
                    adminEmail, errores);
                return;
            }

            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Usuario admin '{AdminEmail}' creado y asignado al rol Admin.", adminEmail);
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
