using Decorations.Application.Interfaces;
using Decorations.Infrastructure.Identity;
using Decorations.Infrastructure.Media;
using Decorations.Infrastructure.Persistence;
using Decorations.Infrastructure.Repositories;
using Decorations.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Decorations.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Admin/Account/Login";
                options.LogoutPath = "/Admin/Account/Logout";
                options.AccessDeniedPath = "/Admin/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IGalleryRepository, GalleryRepository>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IImageProcessingService, ImageProcessingService>();
            services.Configure<ImageProcessingOptions>(
                configuration.GetSection("ImageProcessing"));

            return services;
        }
    }
}
