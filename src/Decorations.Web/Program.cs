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
    builder.Services.AddControllersWithViews(options =>
    {
        // Desactiva el [Required] implícito que ASP.NET Core añade a las propiedades
        // string no anulables cuando #nullable enable está activo. Sin esto, dejar
        // vacíos campos opcionales (Instagram, Facebook, dirección...) invalida el
        // ModelState y el formulario no se guarda. La validación real se hace con FluentValidation.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

    WebApplication app = builder.Build();

    EnsureLogsDirectoryExists(app);

    // Aplica migraciones EF Core y siembra datos base en TODOS los entornos.
    // SeedAsync es idempotente: crea rol/admin/settings solo si faltan.
    // El admin solo se crea si SeedSettings:AdminPassword está definido (en Producción
    // llega por la variable de entorno SeedSettings__AdminPassword del .env del host).
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

void EnsureLogsDirectoryExists(WebApplication application)
{
    string logsDirectory = Path.Combine(application.Environment.ContentRootPath, "logs");
    
    if (!Directory.Exists(logsDirectory))
    {
        Directory.CreateDirectory(logsDirectory);
        Log.Information("Program.cs - Directorio de logs creado en: {LogsPath}", logsDirectory);
    }
    else
    {
        ClearLogsDirectory(logsDirectory);
    }
}

void ClearLogsDirectory(string logsDirectory)
{
    try
    {
        string[] logFiles = Directory.GetFiles(logsDirectory, "log-*.txt");
        
        foreach (string file in logFiles)
        {
            File.Delete(file);
        }

        if (logFiles.Length > 0)
        {
            Log.Information("Program.cs - {LogCount} archivos de logs diarios eliminados para iniciar sesión limpia", logFiles.Length);
        }
    }
    catch (Exception exception)
    {
        Log.Warning(exception, "Program.cs - Error al limpiar directorio de logs");
    }
}
