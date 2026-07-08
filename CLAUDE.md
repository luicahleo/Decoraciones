# CLAUDE.md — Decoraciones Especiales

Guía completa del proyecto para agentes futuros. Lee este archivo antes de tocar cualquier código.

---

## 1. Propósito del proyecto

**Decoraciones Especiales** es una web de portafolio y contacto para un negocio de decoración de eventos (cumpleaños, bodas, bautizos, comuniones, y otros por igual).

### Qué hace la web pública (cliente final)
- Ve trabajos **pasados** organizados en galerías (fotos + vídeos)
- Consulta los servicios que ofrece el negocio
- Envía un **formulario de contacto** (nombre, email, teléfono, tipo de evento, mensaje)
- **El cliente NO crea cuenta.** Solo consulta y contacta.

### Qué hace el panel admin (`/Admin`)
- Crea y gestiona **galerías** de eventos pasados (fotos + vídeos de YouTube)
- Gestiona el **catálogo de servicios** (texto, icono, SEO)
- Lee los **mensajes de contacto** recibidos y los marca como leídos o los elimina
- Edita la **configuración de contacto** del negocio (WhatsApp, email, Instagram, horario)

### Cómo llegan los mensajes al admin
Los mensajes del formulario se guardan en la base de datos. **No hay notificación por email** (pendiente de implementar). El admin debe revisar manualmente el panel en `/Admin/ContactSettings/Messages`.

---

## 2. Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework web | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 + SQL Server |
| Autenticación | ASP.NET Identity |
| Procesamiento de imágenes | SixLabors.ImageSharp 3.1.7 |
| Validación | FluentValidation 12.1.1 |
| Logging | Serilog 9.0.0 (Console + File) |
| Frontend estilos | Tailwind CSS (compilado, no requiere Node en runtime) |
| Frontend JS | Alpine.js 3.14.1 (CDN) |
| Testing | xUnit + Moq |

---

## 3. Arquitectura en capas

```
Decoraciones.sln
├── src/
│   ├── Decorations.Domain          → Entidades y enums (sin dependencias externas)
│   ├── Decorations.Application     → Interfaces, DTOs, Servicios, Validadores
│   ├── Decorations.Infrastructure  → EF Core, Repositorios, Identity, ImageSharp, FileStorage
│   └── Decorations.Web             → Controladores MVC, Vistas Razor, ViewModels, Middleware
└── tests/
    ├── Decorations.UnitTests
    └── Decorations.IntegrationTests
```

**Regla de dependencias:** Domain ← Application ← Infrastructure ← Web

La capa Web no conoce Infrastructure directamente; todo pasa por interfaces definidas en Application.

---

## 4. Entidades del dominio

### GalleryItem
Representa un evento decorado (trabajo pasado). Agrupa varios `MediaAsset`.
```
Id, Title, Description?, EventType?, IsActive, DisplayOrder, ShowAsGrid, CreatedAt
→ ICollection<MediaAsset> (cascade delete)
```
- `ShowAsGrid = true` → todas las fotos en grid
- `ShowAsGrid = false` → foto de portada principal (el thumbnail)

### MediaAsset
Cada foto o vídeo dentro de una galería.
```
Id, GalleryItemId, MediaType (Image|Video), ThumbnailPath?, FullSizePath?,
YoutubeVideoId?, AltText?, DisplayOrder, IsFeatured
```
- Imágenes: se guardan como WebP en dos versiones (thumbnail 600px, full-size 1400px)
- Vídeos: solo se guarda el ID de YouTube; se embebe con iframe

### Service
Servicios que ofrece el negocio, mostrados en la página principal.
```
Id, Title, Description, IconCssClass (emoji), IsActive, DisplayOrder,
SeoMetaTitle, SeoMetaDescription, SeoOpenGraphImageUrl
```

### ContactMessage
Mensajes recibidos del formulario de contacto público.
```
Id, Name, Email, Phone, EventType, Message, ReceivedAt, IsRead
```

### ContactSettings
Único registro de configuración editable por el admin.
```
Id, BusinessName, WhatsAppNumber, Email, InstagramUrl, FacebookUrl, Address, BusinessHours
```

---

## 5. Almacenamiento de imágenes (crítico para VPS)

Las imágenes se guardan en disco en `wwwroot/uploads/`. La ruta física es gestionada por `FileStorageService`, que usa `IWebHostEnvironment.WebRootPath` como base.

### Estructura de carpetas generada
```
wwwroot/uploads/
└── events/
    └── {galleryItemId}/
        ├── thumbnails/   → WebP 600×600, quality 70 (~15-40 KB por imagen)
        └── full-size/    → WebP 1400×1400, quality 85 (~80-250 KB por imagen)
```

### Convención de nombres
`{GuidN}_{nombreOriginal}.webp` — el GUID garantiza unicidad y evita colisiones.

### Proceso de subida
1. Admin sube imagen (máx. 8 MB, cualquier formato)
2. `ImageProcessingService` la convierte a WebP y genera dos versiones en paralelo
3. `FileStorageService` guarda ambos byte arrays en disco y devuelve rutas relativas
4. `MediaAsset` se crea con las rutas relativas (`/uploads/events/{id}/...`)

### Consideraciones de espacio (VPS)
- Cada imagen sube genera ~2 archivos WebP
- Estimación conservadora: 200 KB por imagen (thumbnail + full-size combinado)
- 1.000 fotos ≈ ~200 MB
- **El volumen Docker debe ser persistente.** Ver sección de despliegue.
- Si el espacio es crítico, reducir `WebPQuality` en `appsettings.json` → `ImageProcessing`

### Configuración de límites (appsettings.json)
```json
"ImageProcessing": {
  "MaxFileSizeBytes": 8388608,
  "Thumbnail": { "MaxWidthPixels": 600, "MaxHeightPixels": 600, "WebPQuality": 70 },
  "FullSize":   { "MaxWidthPixels": 1400, "MaxHeightPixels": 1400, "WebPQuality": 85 }
}
```

---

## 6. Vídeos de YouTube

Los vídeos de YouTube se embeben con `<iframe>` estándar. Solo se guarda el **ID del vídeo** (ej: `dQw4w9WgXcQ`), no la URL completa.

### IMPORTANTE: Privado vs. No listado
- **Privado** (`Private`): Solo el dueño de la cuenta puede verlo. **No se puede embeber** para otros usuarios. ❌
- **No listado** (`Unlisted`): Cualquiera con el enlace puede verlo, incluyendo embebidos. ✅

**Los vídeos deben subirse como "No listado" en YouTube**, no como "Privado". Esto garantiza que el cliente pueda verlos embebidos en la web sin necesidad de cuenta de Google.

---

## 7. Flujo de trabajo del admin

### Crear una galería de evento
1. Ir a `/Admin/GalleryManagement/Create`
2. Completar título, descripción, tipo de evento
3. Elegir si mostrar como grid o con portada (`ShowAsGrid`)
4. Arrastrar fotos — la primera se marca como portada por defecto; se puede cambiar
5. Opcionalmente añadir ID de vídeo de YouTube (debe ser "No listado")
6. Guardar → las fotos se convierten a WebP automáticamente

### Gestionar servicios
- `/Admin/ServiceManagement` → CRUD completo de los servicios del negocio

### Leer mensajes
- `/Admin/ContactSettings/Messages` → listado de mensajes ordenados por fecha
- Marcar como leído o eliminar

---

## 8. Despliegue con Docker

### Contenedores
El proyecto usa dos contenedores definidos en `docker-compose.yml`:

| Servicio | Imagen | Puerto |
|---|---|---|
| `sqlserver` | SQL Server 2022 Express | 1433 |
| `app` | Build local (.NET 10) | 8080 |

### Volúmenes persistentes
| Volumen | Montaje en contenedor | Propósito |
|---|---|---|
| `decoraciones-sqlserver-data` | `/var/opt/mssql` | Datos de SQL Server |
| `decoraciones-app-uploads` | `/app/wwwroot/uploads` | Imágenes subidas por el admin |

**Si se elimina el volumen `decoraciones-app-uploads`, se pierden TODAS las imágenes.**

### Variables de entorno del contenedor `app`
```
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=DecorationsDb;...
```
La variable `ConnectionStrings__DefaultConnection` sobreescribe la del `appsettings.json`. En Docker, el host de SQL Server es `sqlserver` (nombre del servicio), no `localhost`.

### Comandos esenciales

```bash
# Construir y levantar todo
docker compose up -d --build

# Solo levantar (sin reconstruir)
docker compose up -d

# Ver logs de la app
docker compose logs -f app

# Detener sin borrar datos
docker compose stop

# Detener y borrar contenedores (los volúmenes persisten)
docker compose down

# ⚠️ PELIGROSO: Borrar todo incluyendo imágenes y base de datos
docker compose down -v
```

### Migraciones y seed
Las migraciones EF Core se aplican **automáticamente al arrancar** la aplicación (`DatabaseSeeder.SeedAsync` llama `MigrateAsync()`). El seed solo corre en `ASPNETCORE_ENVIRONMENT=Development`.

### Credenciales por defecto (solo desarrollo)
- Admin: `admin@decoraciones.com` / `Admin@123456`
- SQL Server SA: `Decoraciones@2024`

---

## 9. Desarrollo local (sin Docker para la app)

Para correr solo la BD en Docker y la app localmente:

```bash
# 1. Levantar solo SQL Server
docker compose up sqlserver -d

# 2. Correr la app (usa appsettings.Development.json con localhost,1433)
dotnet run --project src/Decorations.Web
```

La cadena de conexión en `appsettings.Development.json` ya apunta a `localhost,1433` con SQL auth.

### Desarrollo completo en contenedores

```bash
# Construir imagen y levantar todo
docker compose up -d --build

# App disponible en http://localhost:8080
# Panel admin en http://localhost:8080/Admin
```

### ⚠️ Tailwind CSS está precompilado — recompilar al tocar clases

El CSS de Tailwind se genera a `wwwroot/css/output.css` con un paso de build **manual**. El `Dockerfile` **NO** recompila el CSS: `dotnet publish` copia el `output.css` tal cual está en el repo. Tailwind solo incluye en el output las clases que **encuentra escaneando** los `.cshtml` (ver `content` en `tailwind.config.js`).

**Consecuencia:** si añades o cambias clases de Tailwind en cualquier `.cshtml` y NO recompilas, esas clases faltarán en `output.css` y en producción el elemento saldrá **sin estilos** (aunque en tu editor "se vean" correctas).

Flujo correcto al modificar clases CSS:

```bash
# 1. Recompilar el CSS (escanea los .cshtml y regenera output.css)
cd src/Decorations.Web && npm run build:css

# 2. Reconstruir el contenedor (ya incluye el output.css actualizado)
docker compose up -d --build app
```

- `npm run build:css` → build único minificado. `npm run watch:css` → recompila en caliente durante desarrollo.
- **Commitea siempre `output.css`** junto con los cambios de las vistas.
- Para verificar que una clase entró: `grep -F 'nombre-clase' src/Decorations.Web/wwwroot/css/output.css`.

Los scripts `rebuild.ps1` / `rebuild.sh` de la raíz reconstruyen y levantan el contenedor (no recompilan CSS; hazlo antes si tocaste clases).

---

## 10. Panel admin — URL y acceso

| Ruta | Descripción |
|---|---|
| `/Admin` | Dashboard |
| `/Admin/GalleryManagement` | Galería (CRUD) |
| `/Admin/ServiceManagement` | Servicios (CRUD) |
| `/Admin/ContactSettings` | Configuración de contacto |
| `/Admin/ContactSettings/Messages` | Mensajes recibidos |
| `/Admin/Account/Login` | Login |

La cookie de sesión dura 8 horas con sliding expiration. Lockout tras 5 intentos fallidos (15 minutos).

---

## 11. Páginas públicas

| Ruta | Descripción |
|---|---|
| `/` | Home: hero, servicios, preview galería, CTA |
| `/Gallery` | Galería completa con lightbox (Alpine.js) |
| `/Contact` | Formulario de contacto |
| `/Contact/Confirmation` | Confirmación tras envío |

---

## 12. Inyección de dependencias

### Application (`Decorations.Application/DependencyInjection.cs`)
```csharp
IServiceCatalogService → ServiceCatalogService
IGalleryService        → GalleryService
IContactService        → ContactService
IContactSettingsService → ContactSettingsService
IValidator<ContactMessageDto> → ContactMessageValidator
```

### Infrastructure (`Decorations.Infrastructure/DependencyInjection.cs`)
```csharp
IRepository<T>       → Repository<T>       (genérico)
IGalleryRepository   → GalleryRepository
IFileStorageService  → FileStorageService
IImageProcessingService → ImageProcessingService
```

---

## 13. Convenciones del código

- **Idioma del código:** inglés (nombres de clases, métodos, propiedades)
- **Idioma de logs/comentarios:** español
- **Mapeo:** Manual entre entidades ↔ DTOs (sin AutoMapper). Métodos privados estáticos `MapToDto()`, `MapToNewEntity()`, `UpdateEntityFromDto()`
- **Async:** Todo el acceso a datos es `async/await`
- **CSRF:** `[ValidateAntiForgeryToken]` en todos los POST
- **Null safety:** `#nullable enable` en todos los proyectos

---

## 14. Limitaciones conocidas y pendientes

### Bugs corregidos (ya aplicados)
- `ContactSettingsService.UpdateContactSettingsAsync`: Buscaba por `Id = 0` cuando no había settings → corregido para buscar `FirstOrDefault()`
- `GalleryManagementController.Create`: `featuredImageIndex` no inicializaba a `-1` → podía marcar primera imagen como portada sin quererlo

### Pendientes de implementar
| Feature | Prioridad | Notas |
|---|---|---|
| Notificación por email al recibir mensaje | Alta | Admin no sabe que llegó un mensaje sin entrar al panel |
| Rate limiting en formulario de contacto | Media | Sin CAPTCHA ni throttling actualmente |
| CAPTCHA en formulario de contacto | Media | Vulnerable a spam |
| Paginación en galería pública | Media | Carga todos los registros de una vez |
| CSP headers | Baja | No hay Content-Security-Policy configurado |
| HTTPS enforcement en código | Baja | Solo configurado en infra, no en app |

### Nota sobre el GlobalExceptionMiddleware
Cuando hay excepción HTML, el middleware setea status 500 y luego intenta redirigir a `/Home/Error`. Algunos navegadores no siguen el redirect si el status ya es 500. Pendiente de revisar.

---

## 15. Logging

Serilog configurado por entorno:

- **Development:** Console (debug) + archivo diario `logs/log-*.txt` + `logs/all-logs.txt`
- **Production:** Solo archivo diario, nivel Warning, retiene 30 días

Los logs diarios se eliminan al arrancar la app (limpieza en `Program.cs → ClearLogsDirectory`). El `all-logs.txt` NO se limpia.

---

## 16. Archivos clave de referencia rápida

| Propósito | Archivo |
|---|---|
| Arranque y middleware | `src/Decorations.Web/Program.cs` |
| Schema de BD y relaciones | `src/Decorations.Infrastructure/Persistence/ApplicationDbContext.cs` |
| Seed inicial (admin + settings) | `src/Decorations.Infrastructure/Persistence/Seed/DatabaseSeeder.cs` |
| Procesamiento de imágenes | `src/Decorations.Infrastructure/Media/ImageProcessingService.cs` |
| Guardado de archivos en disco | `src/Decorations.Infrastructure/Storage/FileStorageService.cs` |
| Galería (lógica principal) | `src/Decorations.Application/Services/GalleryService.cs` |
| Config de imágenes | `src/Decorations.Web/appsettings.json` → sección `ImageProcessing` |
| Entidades del dominio | `src/Decorations.Domain/Entities/` |
| Interfaces de servicios | `src/Decorations.Application/Interfaces/` |
| Docker (ambos contenedores) | `docker-compose.yml` |
| Imagen Docker de la app | `Dockerfile` |
