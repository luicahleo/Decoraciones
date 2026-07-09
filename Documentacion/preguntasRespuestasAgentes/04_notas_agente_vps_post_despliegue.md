# Notas del agente del VPS → agente local (post-despliegue)

**Fecha:** 2026-07-09
**Estado:** ✅ **Decoraciones desplegado y ONLINE** en https://decoraciones.trajano.online

Esta nota resume cómo quedó el despliegue, un **hallazgo importante de código** que requiere tu acción,
y los pendientes acordados.

---

## 1. Resultado del despliegue (verificado)

| Componente | Estado |
|---|---|
| App en `https://decoraciones.trajano.online` | ✅ responde 200, TLS válido, redirect 80→443 |
| Contenedor `decoraciones` | ✅ Up (healthy), `127.0.0.1:8082→8080` |
| Migraciones EF Core (13 tablas) | ✅ aplicadas al primer arranque |
| Usuario admin + rol `Admin` | ✅ creado (ver §2 — costó un intento) |
| BD `DecoracionesDB` + usuario dedicado `decoraciones_app` (`db_owner`) | ✅ |
| CI/CD (GitHub Actions → SSH → build en VPS) | ✅ funcionó de punta a punta |
| Imágenes persistidas (bind mount `uploads/`) y agrupadas por `galleryItemId` | ✅ |
| Certificado Let's Encrypt + renovación automática | ✅ |
| Clave SSH dedicada `github-actions-decoraciones` autorizada | ✅ |

Las credenciales reales (admin y usuario de BD) están en `/var/apps/decoraciones/.env` (permisos 600) en
el servidor y se comunicaron por separado. **No se incluyen aquí a propósito** (este archivo no debe llevar
secretos).

---

## 2. ⚠️ HALLAZGO IMPORTANTE — el `DatabaseSeeder` traga fallos en silencio (requiere tu acción)

En el primer despliegue **el admin NO se creó**, aunque las migraciones y el rol `Admin` sí. Diagnóstico:

- La config de Identity (`DependencyInjection.cs`) exige `Password.RequireNonAlphanumeric = true`
  (además de longitud ≥8, mayúscula, minúscula y dígito).
- La primera contraseña de admin generada no tenía símbolo → `userManager.CreateAsync(...)` **falló**.
- Pero en `DatabaseSeeder.SeedAdminUserAsync` (líneas 62-66):

  ```csharp
  IdentityResult createResult = await userManager.CreateAsync(adminUser, adminPassword);
  if (createResult.Succeeded)
  {
      await userManager.AddToRoleAsync(adminUser, "Admin");
  }
  // <-- si NO tiene éxito, no se loguea ni se lanza nada: el fallo se traga
  ```

  Resultado: el rol se crea, el usuario no, y **nadie se entera**. Solo lo detecté consultando
  `AspNetUsers` directamente en la BD.

**Corrección recomendada** (repo, no VPS): registrar/propagar el error cuando falla la creación. Ejemplo:

```csharp
IdentityResult createResult = await userManager.CreateAsync(adminUser, adminPassword);
if (!createResult.Succeeded)
{
    string errores = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
    // logger inyectado, o al menos:
    throw new InvalidOperationException($"No se pudo crear el usuario admin '{adminEmail}': {errores}");
}
await userManager.AddToRoleAsync(adminUser, "Admin");
```

Así, un `.env` con una contraseña que no cumple la política **falla ruidosamente en el arranque** en vez de
dejar la app sin admin de forma silenciosa.

> Se resolvió en producción poniendo una contraseña de admin que **sí** cumple la política (incluye símbolo)
> y recreando el contenedor. El seeder es idempotente (busca el usuario por email), así que reintentó y creó
> el admin correctamente. La app ya tiene su admin.

**Nota operativa para el futuro:** cualquier contraseña de admin definida en `SeedSettings__AdminPassword`
debe cumplir: ≥8 chars, mayúscula, minúscula, dígito **y al menos un carácter no alfanumérico**.

---

## 3. Pendientes acordados (no bloquean la operación actual)

- **Backups (pospuesto por decisión del humano):** hoy existen scripts + cron para BD (`DecoracionesDB`,
  retención 30 d) e imágenes (`uploads/`, tar.gz diario, retención 14 d). Se revisarán más adelante.
  - ⚠️ **Aviso de diseño para cuando se retome:** el backup de imágenes hace un **tar.gz completo cada día**.
    Como las imágenes son inmutables y los `.webp` no comprimen, con retención larga (p. ej. 1 año) el disco
    se llenaría (coste ≈ tamaño_fotos × nº_días). La solución propuesta es **snapshots incrementales con
    `rsync --link-dest`** (365 puntos de restauración por el coste de guardar las fotos ~una vez). Las BD, en
    cambio, sí pueden ir a retención larga sin problema (son pocos MB).
  - No hay copia **off-site**: todo vive en el mismo disco del VPS. Para desastre real (fallo de disco) haría
    falta copiar fuera del servidor.

- **Logrotate de `logs/`:** ❌ **no necesario.** Serilog ya rota diario (`rollingInterval: Day`) y retiene 30
  archivos (`retainedFileCountLimit: 30`). Añadir logrotate sería redundante. Si se quiere más histórico,
  subir `retainedFileCountLimit` en `appsettings` (código), no meter logrotate.

---

## 4. Referencia rápida del entorno en el VPS

- Ruta de la app: `/var/apps/decoraciones/` (`web/` = binarios del rsync, `uploads/`, `logs/`, `backups/`,
  `Dockerfile.web`, `docker-compose.yml`, `.env`).
- SQL Server compartido: contenedor `trajano-sqlserver` (alias de red `trajano-sqlserver` / `sqlserver`),
  puerto 1433 **solo accesible por VPN WireGuard**.
- Red Docker: `trajano-shared-network` (externa).
- Deploy: push a `main` o `workflow_dispatch` en el repo Decoraciones. El runner compila, hace rsync a
  `web/` y por SSH reconstruye la imagen y levanta el contenedor. **Los redeploys NO borran `uploads/` ni
  `logs/`** (son bind mounts del host).
