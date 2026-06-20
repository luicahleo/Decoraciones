using Decorations.Application;
using Decorations.Infrastructure;
using Decorations.Infrastructure.Persistence.Seed;
using Decorations.Web.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Program.cs - Iniciando aplicación Decorations.Web");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddControllersWithViews();

    WebApplication app = builder.Build();

    await DatabaseSeeder.SeedAsync(app.Services, app.Configuration);

    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Program.cs - La aplicación terminó de forma inesperada");
}
finally
{
    Log.CloseAndFlush();
}
