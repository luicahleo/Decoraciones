# Preguntas para el agente del VPS — Despliegue de Decoraciones

> **Objetivo:** desplegar la aplicación **Decoraciones Especiales** (.NET 10 / ASP.NET Core MVC) en el VPS,
> en su propio contenedor Docker, servida detrás del proxy nginx bajo el subdominio
> **`decoraciones.trajano.online`** (ya creado en el panel de IONOS del dominio `trajano.online`),
> reutilizando el contenedor de **SQL Server compartido** que ya existe y usando **GitHub Actions** para CI/CD.

> **Instrucciones para el agente del VPS:** responde **debajo de cada pregunta**, en la línea que empieza por `➡️ RESPUESTA:`.
> Si algo no aplica o no existe todavía, dilo explícitamente (no lo dejes en blanco).
> Cuando puedas, pega la salida real de comandos (`docker ps`, `docker network ls`, contenido de archivos de config, etc.)
> en bloques de código, en lugar de describirla de memoria.

---

## Contexto que YA conozco (para que confirmes o corrijas)

El repositorio trae hoy un `docker-compose.yml` que levanta **su propio** SQL Server y la app juntos.
Para el VPS esto **cambiará**: la app debe conectarse al **SQL Server compartido existente**, no levantar uno nuevo.
El `Dockerfile` compila y publica la app .NET 10 y expone el puerto **8080** dentro del contenedor.

Necesito tus respuestas para adaptar: `docker-compose.yml`, la config de nginx, el workflow de GitHub Actions,
las rutas de volúmenes en el host y la gestión de secretos.

---

## Bloque 1 — Información general del VPS

1. ¿Cuál es el **sistema operativo y versión** del VPS? (ej: Ubuntu 22.04)
   ➡️ RESPUESTA:

2. ¿Qué **versiones** de Docker y Docker Compose hay instaladas? (`docker --version`, `docker compose version`)
   ➡️ RESPUESTA:

3. ¿La app se despliega con **`docker compose`** (v2, plugin) o `docker-compose` (v1, binario)?
   ➡️ RESPUESTA:

4. ¿Cuál es la **ruta base en el host** donde vive el código/los proyectos desplegados?
   (ej: `/opt/apps/`, `/home/deploy/`, `/srv/`). ¿Dónde debería colocar la carpeta de `decoraciones`?
   ➡️ RESPUESTA:

5. ¿Qué **usuario** ejecuta los contenedores y los despliegues? ¿Tiene permisos de Docker sin `sudo`?
   ➡️ RESPUESTA:

6. ¿Cuánto **espacio en disco libre** hay disponible? (las imágenes subidas se acumulan en un volumen; estimamos ~200 KB por foto)
   ➡️ RESPUESTA:

---

## Bloque 2 — SQL Server compartido

7. ¿Cuál es el **nombre exacto del contenedor** de SQL Server compartido? (`docker ps` → columna NAMES)
   ➡️ RESPUESTA:

8. ¿Qué **imagen y versión** usa ese SQL Server? (ej: `mcr.microsoft.com/mssql/server:2022-latest`)
   ➡️ RESPUESTA:

9. ¿A qué **red(es) Docker** está conectado ese contenedor? (`docker network ls` y `docker inspect <contenedor> --format '{{json .NetworkSettings.Networks}}'`)
   ➡️ RESPUESTA:

10. ¿Cuál es el **hostname/servicio** por el que otros contenedores se conectan a SQL Server dentro de la red Docker?
    (En el compose actual la app usa `Server=sqlserver,1433`; necesito el nombre real en tu red.)
    ➡️ RESPUESTA:

11. ¿SQL Server escucha en el **puerto 1433** interno estándar? ¿Está ese puerto expuesto al host o solo accesible dentro de la red Docker?
    ➡️ RESPUESTA:

12. **Credenciales / usuario de BD:** ¿quieres que la app use el usuario `sa`, o prefieres que creemos un
    **usuario dedicado** con permisos solo sobre su base de datos? (recomendado por seguridad). ¿Cómo me pasas la contraseña de forma segura?
    ➡️ RESPUESTA:

13. ¿Debo **crear la base de datos** `DecorationsDb` yo mismo, o el contenedor de SQL Server la crea al vuelo?
    (La app aplica migraciones EF Core automáticamente al arrancar, pero la BD/el login deben poder crearse.)
    ➡️ RESPUESTA:

14. ¿Hay alguna **convención de nombres** para las bases de datos en ese SQL Server compartido que deba respetar?
    (para no chocar con otras apps)
    ➡️ RESPUESTA:

15. ¿Existe **backup automático** de ese SQL Server? Si es así, ¿la nueva BD entraría en el backup automáticamente?
    ➡️ RESPUESTA:

---

## Bloque 3 — Red Docker y conectividad entre contenedores

16. ¿Prefieres que la app se **una a una red existente** (la del SQL Server y/o la del nginx) o que cree su propia red y la conecte?
    ➡️ RESPUESTA:

17. ¿El contenedor de **nginx** y el de **SQL Server** están en la **misma red** o en redes separadas?
    (Necesito que la app pueda hablar con ambos.)
    ➡️ RESPUESTA:

18. Lista de **redes Docker existentes** relevantes con su nombre y tipo (`docker network ls`):
    ```
    (pega aquí la salida)
    ```
    ➡️ RESPUESTA:

---

## Bloque 4 — Proxy nginx y dominio/SSL

19. ¿El nginx corre como **contenedor** o instalado directamente en el host? Si es contenedor, ¿cuál es su nombre?
    ➡️ RESPUESTA:

20. ¿Cómo se gestionan hoy los **virtual hosts / server blocks**? ¿Hay una carpeta de configs por sitio
    (ej: `/etc/nginx/conf.d/` o `sites-available`) o usas algo como **nginx-proxy** / **Traefik** con labels?
    ➡️ RESPUESTA:

21. Pega la **config de nginx de un subdominio ya funcionando** (otra app), para copiar el patrón exacto
    (proxy_pass, headers, tamaño máximo de body, etc.):
    ```
    (pega aquí un server block de ejemplo)
    ```
    ➡️ RESPUESTA:

22. **SSL/TLS:** ¿usas **Let's Encrypt / Certbot**? ¿El certificado de `decoraciones.trajano.online` ya está emitido,
    o hay que generarlo? ¿El proceso es automático (companion) o manual?
    ➡️ RESPUESTA:

23. El registro **DNS** de `decoraciones.trajano.online` en IONOS, ¿ya apunta a la IP del VPS (registro A)?
    ¿Es un A directo o un CNAME al dominio principal?
    ➡️ RESPUESTA:

24. La app sube imágenes de hasta **8 MB**. ¿El nginx tiene configurado `client_max_body_size` suficiente,
    o debo indicarte que lo suba para este subdominio? (recomiendo al menos `12M`)
    ➡️ RESPUESTA:

25. ¿A qué **puerto del host** debo publicar la app para que nginx la alcance, o nginx la alcanza por
    **nombre de contenedor dentro de la red Docker** (sin publicar puerto al host)?
    (En el compose actual publica `8080:8080`; dime tu preferencia para evitar colisión de puertos.)
    ➡️ RESPUESTA:

---

## Bloque 5 — GitHub Actions / CI-CD

26. ¿Cómo llega hoy el código al VPS en tus otras apps? Opciones típicas:
    (a) el runner hace **SSH al VPS** y ejecuta `git pull` + `docker compose up -d --build`;
    (b) se construye una **imagen** y se publica en un **registry** (GHCR/Docker Hub) y el VPS hace `pull`;
    (c) **self-hosted runner** dentro del VPS. ¿Cuál usas?
    ➡️ RESPUESTA:

27. Si usas **registry**: ¿cuál (GHCR, Docker Hub, otro)? ¿Cuál es la convención de nombres de imagen y cómo se autentica el VPS para hacer pull?
    ➡️ RESPUESTA:

28. Si usas **SSH deploy**: ¿qué **secrets** existen ya en GitHub (host, usuario, clave SSH, puerto)? ¿Cómo los nombras?
    ➡️ RESPUESTA:

29. ¿Puedes pegar un **workflow `.yml` existente** de otra app como plantilla?
    ```yaml
    (pega aquí un workflow de ejemplo)
    ```
    ➡️ RESPUESTA:

30. ¿En qué **rama** se dispara el deploy? (¿`main`? ¿tag? ¿manual con `workflow_dispatch`?)
    ➡️ RESPUESTA:

31. **Recompilación de Tailwind CSS:** el proyecto necesita `npm run build:css` antes de publicar si se tocan clases.
    ¿El pipeline debe correr Node/npm para regenerar el CSS, o commiteamos `output.css` y el build solo hace `dotnet publish`?
    (Hoy el Dockerfile **no** recompila CSS; asume que `output.css` ya está en el repo.)
    ➡️ RESPUESTA:

---

## Bloque 6 — Variables de entorno y secretos

32. La app necesita como mínimo `ASPNETCORE_ENVIRONMENT` y `ConnectionStrings__DefaultConnection`.
    ¿Cómo gestionas los **secretos** en el VPS? (archivo `.env` no versionado, Docker secrets, variables en el compose, gestor externo)
    ➡️ RESPUESTA:

33. ¿Debe la app correr como `ASPNETCORE_ENVIRONMENT=Production`?
    ⚠️ **Importante:** hoy el seed de datos (crea el usuario admin inicial) **solo corre en `Development`**.
    Si va a `Production`, necesito ajustar el arranque para poder crear el admin la primera vez. ¿Cómo lo prefieres?
    ➡️ RESPUESTA:

34. Credenciales del **admin de la app** en producción: la semilla por defecto es `admin@decoraciones.com` / `Admin@123456`.
    ¿Quieres cambiarlas para producción? ¿Me pasas las definitivas por un canal seguro?
    ➡️ RESPUESTA:

---

## Bloque 7 — Persistencia, volúmenes y logs

35. Las imágenes subidas viven en un volumen montado en `/app/wwwroot/uploads`.
    ¿Prefieres un **volumen Docker con nombre** (como ahora: `decoraciones-app-uploads`) o un **bind mount** a una ruta del host
    que entre en tus backups? Si es bind mount, ¿qué ruta?
    ➡️ RESPUESTA:

36. ¿Hay **backup** de volúmenes/carpetas en el VPS? ¿Debo colocar los uploads en una ruta específica para que entren en el backup?
    ➡️ RESPUESTA:

37. Los logs de la app se escriben en `/app/logs` (Serilog, archivo diario). ¿Los quieres persistidos en el host, o basta con
    `docker compose logs`? ¿Tienes rotación/limpieza centralizada de logs?
    ➡️ RESPUESTA:

---

## Bloque 8 — Operación y varios

38. ¿Usas alguna **política de reinicio** estándar para los contenedores? (el compose actual usa `restart: unless-stopped`)
    ➡️ RESPUESTA:

39. ¿Hay **monitorización / healthchecks** centralizados (Uptime Kuma, Portainer, etc.) donde deba registrar este servicio?
    ➡️ RESPUESTA:

40. ¿Existe alguna **convención de nombres de contenedor** que deba seguir? (hoy propongo `decoraciones-app`)
    ➡️ RESPUESTA:

41. ¿Alguna **restricción de recursos** (límites de CPU/memoria por contenedor) que deba declarar en el compose?
    ➡️ RESPUESTA:

42. ¿Algo más del entorno del VPS que deba saber y no haya preguntado? (firewall, fail2ban, zona horaria del servidor, etc.)
    ➡️ RESPUESTA:

---

> **Cuando termines de responder**, guarda este archivo (o crea `02_respuestas_agente_vps.md` con las respuestas)
> en esta misma carpeta. Con eso adaptaré el `docker-compose.yml`, la config de nginx, el workflow de GitHub Actions
> y las variables de entorno para dejar el despliegue listo.
