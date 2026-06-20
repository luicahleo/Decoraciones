# 🎉 Plan de Implementación — Web de Decoración de Eventos

**Stack:** .NET 10 (ASP.NET Core MVC) · EF Core · SQL Server · Tailwind CSS · Alpine.js/HTMX
**Arquitectura:** Monolítica MVC con separación por capas (SOLID + Clean Code)
**Convención:** Código en inglés · Comentarios y docs en español

---

## 📐 Visión General de la Arquitectura

Aunque es un **monolito MVC**, internamente separaremos responsabilidades por capas
para mantener bajo acoplamiento y alta cohesión:

```
Decoraciones.sln
│
├── src/
│   ├── Decorations.Web            → Capa de presentación (MVC: Controllers, Views, wwwroot)
│   ├── Decorations.Application    → Lógica de negocio (Services, DTOs, Interfaces, Validators)
│   ├── Decorations.Domain         → Entidades, Enums, contratos de dominio (sin dependencias)
│   └── Decorations.Infrastructure → EF Core, Repositorios, Identity, Almacenamiento, Logging
│
└── tests/
    ├── Decorations.UnitTests        → xUnit + Moq/NSubstitute
    └── Decorations.IntegrationTests → EF Core InMemory / SQLite
```

> **Nota de diseño:** El flujo de dependencias siempre apunta hacia el `Domain`
> (Dependency Inversion). `Web` e `Infrastructure` dependen de `Application`/`Domain`,
> nunca al revés.

---

## 🚀 FASE 1 — Setup, Arquitectura Base y Observabilidad

**Objetivo:** Dejar el esqueleto de la solución compilando, con logging y manejo
global de errores funcionando.

### Componentes a crear
- **Solución y proyectos** (4 proyectos `src` + 2 de `tests`) referenciados correctamente.
- **Configuración de `Program.cs`** con el patrón mínimo de hosting de .NET 10.
- **Logging estructurado (Serilog):**
  - Sinks: Console (Dev) + Rolling File (`logs/log-.txt`, rotación diaria).
  - Enriquecedores: `MachineName`, `ThreadId`, `CorrelationId`.
  - Configuración por entorno (`appsettings.Development.json` vs `appsettings.json`).
- **Global Exception Handling:**
  - Middleware personalizado `GlobalExceptionMiddleware` que captura excepciones no
    manejadas, registra el stack trace y devuelve una vista/respuesta amigable.
  - Página de error genérica para producción.
- **Rutas relativas:** configuración base para garantizar portabilidad (sin URLs absolutas).

### Carpetas / archivos clave
```
Decorations.Web/
├── Program.cs
├── appsettings.json / appsettings.Development.json
└── Middleware/GlobalExceptionMiddleware.cs
logs/   (generado, ignorado en .gitignore)
```

**✅ Criterio de validación:** La app arranca, escribe logs en archivo y consola,
y un error provocado se registra con stack trace completo.

---

## 🗄️ FASE 2 — Modelos de Dominio, DbContext e Identity

**Objetivo:** Modelo de datos persistente y autenticación lista.

### Componentes a crear
- **Entidades de Dominio** (`Decorations.Domain/Entities`):
  - `Service` (catálogo de servicios + datos SEO).
  - `GalleryItem` (foto o video, con tipo de medio).
  - `MediaAsset` (rutas de imágenes procesadas / IDs de YouTube).
  - `ContactSettings` (datos de contacto editables: WhatsApp, email, redes).
  - `ContactMessage` (mensajes del formulario público).
  - Enums: `MediaType { Image, Video }`.
- **ASP.NET Core Identity:**
  - `ApplicationUser : IdentityUser` para administradores.
  - Configuración de políticas de contraseña, lockout y cookies.
- **EF Core:**
  - `ApplicationDbContext : IdentityDbContext<ApplicationUser>`.
  - Configuraciones por entidad con `IEntityTypeConfiguration<T>` (Fluent API).
  - Cadena de conexión SQL Server por entorno.
- **Migración inicial** + **Seeder** (rol `Admin`, usuario administrador inicial,
  datos de contacto por defecto).

### Carpetas / archivos clave
```
Decorations.Domain/Entities/
Decorations.Infrastructure/
├── Persistence/ApplicationDbContext.cs
├── Persistence/Configurations/*.cs
├── Identity/ApplicationUser.cs
└── Persistence/Seed/DatabaseSeeder.cs
```

**✅ Criterio de validación:** `dotnet ef database update` crea la BD,
las tablas de Identity y de dominio existen, y el seed crea el admin.

---

## ⚙️ FASE 3 — Servicios, Repositorios e Inyección de Dependencias

**Objetivo:** Encapsular la lógica de negocio detrás de interfaces, lista para testear.

### Componentes a crear
- **Contratos (interfaces) en `Application`:**
  - `IServiceCatalogService`, `IGalleryService`, `IContactService`, `IContactSettingsService`.
  - `IImageProcessingService` (redimensión + conversión a WebP + validación de peso).
  - `IFileStorageService` (almacenamiento físico local con rutas relativas).
- **Repositorios (patrón Repository + Unit of Work):**
  - `IRepository<T>` genérico + repositorios específicos en `Infrastructure`.
- **Implementaciones de servicios** en `Application`/`Infrastructure` según corresponda.
- **Procesamiento de imágenes (SixLabors.ImageSharp):**
  - Redimensionado, conversión a `.webp`, validación de tamaño máximo configurable.
- **DTOs y mapeo** (manual o AutoMapper) entre entidades y ViewModels/DTOs.
- **Validación** con FluentValidation (sanitización de inputs del formulario).
- **Registro de DI:** clase de extensión `ServiceCollectionExtensions`
  (`AddApplicationServices()`, `AddInfrastructureServices()`) para mantener
  `Program.cs` limpio.

### Carpetas / archivos clave
```
Decorations.Application/
├── Interfaces/
├── Services/
├── DTOs/
├── Validators/
└── DependencyInjection.cs
Decorations.Infrastructure/
├── Repositories/
├── Storage/FileStorageService.cs
├── Media/ImageProcessingService.cs
└── DependencyInjection.cs
```

**✅ Criterio de validación:** Todos los servicios resuelven vía DI;
subir una imagen genera un `.webp` redimensionado en disco.

---

## 🎨 FASE 4 — Controladores y Frontend MVC

**Objetivo:** Vistas públicas y panel de administración funcionales.

### 4A. Vista Pública (Front-end)
- **Controladores:** `HomeController`, `ServicesController`, `GalleryController`, `ContactController`.
- **Vistas (Razor):**
  - **Landing Page** con catálogo visual de servicios.
  - **SEO:** partial `_SeoMeta` con metaetiquetas + Open Graph dinámicas por página.
  - **Galería:** grid optimizado con **lazy loading** y **lightbox** (Alpine.js).
  - **Videos:** embed/iframe de YouTube a partir del ID guardado.
  - **Formulario de contacto** con sanitización + **antiforgery token (CSRF)**.
  - **Botón flotante de WhatsApp** (enlace `wa.me` con número configurable).
- **Tailwind CSS:**
  - Setup con pipeline (`npm` + `tailwind.config.js`), diseño **Mobile-First**.
  - **Dark/Light Mode** con `class` strategy + persistencia en `localStorage` (Alpine.js).

### 4B. Panel de Administración (CMS)
- **Área `Admin`** protegida con `[Authorize(Roles = "Admin")]`.
- **Controladores:** `DashboardController`, `GalleryManagementController`, `ContactSettingsController`, `AccountController` (login/logout).
- **Vistas responsivas** con Dark/Light mode:
  - **CRUD de Galerías** (subida de imágenes → procesamiento WebP; alta de videos por ID).
  - **Configuración de contacto** dinámica.
- **Seguridad transversal:**
  - Antiforgery global, headers de seguridad (CSP, X-Content-Type-Options),
    HTML encoding por defecto de Razor (anti-XSS), EF Core parametrizado (anti-SQLi).

### Carpetas / archivos clave
```
Decorations.Web/
├── Controllers/
├── Areas/Admin/Controllers/
├── Areas/Admin/Views/
├── Views/{Home,Services,Gallery,Contact,Shared}/
├── ViewModels/
└── wwwroot/{css,js,uploads,images}/
```

**✅ Criterio de validación:** Navegación pública completa, login admin funcional,
CRUD de galería operativo, modo oscuro persistente, formulario protegido contra CSRF.

---

## 🧪 FASE 5 — Pruebas y Aseguramiento de Calidad

**Objetivo:** Cobertura de lógica de negocio e integración con datos.

### Componentes a crear
- **Pruebas Unitarias (`Decorations.UnitTests`):**
  - xUnit + Moq/NSubstitute.
  - Tests de servicios (`GalleryService`, `ImageProcessingService`, `ContactService`).
  - Tests de validadores y de controladores (mockeando dependencias).
- **Pruebas de Integración (`Decorations.IntegrationTests`):**
  - EF Core **InMemory** o **SQLite in-memory** para repositorios y consultas.
  - `WebApplicationFactory<Program>` para tests de endpoints (smoke tests).
- **Datos de prueba:** builders/fixtures reutilizables.

### Carpetas / archivos clave
```
tests/Decorations.UnitTests/
├── Services/
├── Validators/
└── Controllers/
tests/Decorations.IntegrationTests/
├── Repositories/
└── Endpoints/
```

**✅ Criterio de validación:** `dotnet test` en verde; lógica de negocio crítica cubierta.

---

## 📋 Resumen de Fases

| Fase | Foco | Entregable clave |
|------|------|------------------|
| **1** | Setup + Logs | Solución compilando, Serilog + manejo global de errores |
| **2** | Datos + Identity | DbContext, entidades, migración inicial, admin seed |
| **3** | Lógica + DI | Servicios, repositorios, ImageSharp/WebP, DI configurada |
| **4** | UI MVC | Vistas públicas (SEO/galería/WhatsApp) + CMS admin seguro |
| **5** | Testing | Suite unitaria + integración en verde |

---

## ✅ Decisiones Técnicas Confirmadas

| Decisión | Elección | Motivo |
|----------|----------|--------|
| **Frontend JS** | Alpine.js (única librería) | Suficiente para dark mode, lightbox y CMS; evita mezclar dos librerías |
| **Patrón de datos** | Repository + Unit of Work | Facilita testing con mocks y respeta inversión de dependencias |
| **Mapeo de objetos** | Manual (sin AutoMapper) | Control total, cero "magia", sin dependencia extra |
| **Validación** | FluentValidation | Separación clara de responsabilidades; reglas en clases propias y testeables |

---

> **Plan aprobado. Comenzamos con la Fase 1.**
