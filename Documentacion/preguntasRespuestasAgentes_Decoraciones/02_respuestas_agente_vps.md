# Respuestas del agente del VPS — Despliegue de Decoraciones

**Fecha:** 2026-07-09
**Host:** `ubuntu` — Ubuntu 24.04.2 LTS — kernel 6.8.0-59-generic — x86_64
**Alcance de este diagnóstico:** solo lectura del sistema. No se creó ningún contenedor, BD, red ni
config de nginx todavía. Todo lo de abajo es el estado real verificado con comandos.
**IP pública del VPS:** `194.164.171.217`

> Referencia usada: la app **icarus** (`.NET 10`, mismo patrón exacto que decoraciones) ya está
> desplegada aquí con GitHub Actions + SSH + nginx en host + SQL Server compartido. Reutilizo ese
> patrón porque funciona y decoraciones encaja igual.

---

## Bloque 1 — Información general del VPS

**1. SO y versión**
`Ubuntu 24.04.2 LTS (Noble Numbat)`, kernel `6.8.0-59-generic`, x86_64. Es una VM KVM.

**2. Versiones de Docker / Compose**
```
Docker version 28.1.1, build 4eba377
Docker Compose version v2.35.1        (plugin v2)
docker-compose version 1.29.2         (binario v1, también presente)
```

**3. ¿`docker compose` v2 o `docker-compose` v1?**
Están **ambos**. Usa **`docker compose` (v2, plugin)** — es lo recomendado. El v1 sigue instalado solo
por compatibilidad con scripts viejos (los scripts de icarus lo detectan y caen a uno u otro). Para
decoraciones usaremos v2.

**4. Ruta base en el host**
Las apps viven en **`/var/apps/`** (icarus, modassussy, vitiligoenter, clinicasuecia, infrastructure…).
La carpeta de decoraciones debe ir en **`/var/apps/decoraciones/`**. (Hoy solo existe
`/var/apps/preguntasRespuestasAgente/` con estas preguntas; la carpeta real de la app aún no está creada.)

**5. Usuario que ejecuta contenedores/despliegues**
**`root`**. Docker funciona sin `sudo` para root. El deploy de GitHub Actions también entra por SSH como
`root` (ver Bloque 5). No hay usuario `deploy` dedicado.

**6. Espacio en disco libre**
```
Filesystem      Size  Used Avail Use% Mounted on
/dev/vda1       232G   90G  143G  39% /
```
**~143 GB libres.** Docker vive en el mismo disco (`/`). A ~200 KB/foto sobra de lejos (143 GB ≈ 700k
fotos). Sin problema.

---

## Bloque 2 — SQL Server compartido

**7. Nombre exacto del contenedor**
**`trajano-sqlserver`**. `Up 2 months`.

**8. Imagen y versión**
`mcr.microsoft.com/mssql/server:2019-latest` — **edición Express** (`MSSQL_PID=Express`).
⚠️ Ojo con los topes de Express: **máx. 10 GB por base de datos**, ~1.4 GB de buffer pool, 4 cores. Para
decoraciones (catálogo + fotos en volumen, no en BD) es más que suficiente, pero tenlo presente.

**9. Redes Docker del contenedor SQL**
Solo **`trajano-shared-network`** (bridge, subred `172.18.0.0/16`), IP `172.18.0.2`. Alias de red:
`trajano-sqlserver` **y `sqlserver`**.
```json
{"trajano-shared-network": {"IPAddress":"172.18.0.2",
  "DNSNames":["trajano-sqlserver","sqlserver","f6096ee22507"]}}
```

**10. Hostname/servicio para conectarse dentro de la red**
Cualquiera de los dos alias resuelve: **`trajano-sqlserver`** o **`sqlserver`**. Como el compose actual de
decoraciones usa `Server=sqlserver,1433`, **funcionará tal cual** gracias al alias `sqlserver` — no hace
falta cambiar la cadena por ese motivo. (icarus usa `Server=trajano-sqlserver`; ambos valen.)

**11. Puerto 1433**
Sí, interno estándar **1433**. Está publicado en el host como `0.0.0.0:1433`, **pero NO está expuesto a
Internet**: reglas en la cadena `DOCKER-USER` (persistidas) solo permiten la VPN WireGuard:
```
-A DOCKER-USER ! -s 10.8.0.0/24 -p tcp -m tcp --dport 1433 -j DROP
-A DOCKER-USER -s 10.8.0.0/24 -p tcp -m tcp --dport 1433 -j ACCEPT
```
Para decoraciones da igual: la app hablará con SQL **por la red Docker** (`sqlserver,1433`), no por el
puerto publicado. Para administrar la BD desde fuera (SSMS/Azure Data Studio) hay que estar en la VPN.

**12. Credenciales / usuario de BD**
Hoy todo el mundo usa `sa` con contraseña **`Password123!`** (está en texto plano en
`/var/apps/infrastructure/docker-compose.yml` — la conozco). **Recomiendo fuertemente un usuario
dedicado** `decoraciones_app` con permisos solo sobre `DecoracionesDB` (`db_owner` de esa BD, para que EF
Core pueda migrar). Yo puedo crearlo. Necesito que me pases **la contraseña del nuevo usuario por un canal
seguro** (no la pongas en el repo); la meteré en el `.env` del host (ver Bloque 6). Si prefieres rapidez y
menos seguridad, se puede usar `sa`, pero no lo aconsejo.

**13. ¿Creo yo la BD `DecorationsDb`?**
**Sí, la creo yo** (el contenedor de SQL no crea BDs al vuelo). El flujo será: creo la BD + el login/usuario
dedicado, y luego la app aplica migraciones EF Core al arrancar (el usuario tendrá `db_owner` sobre esa BD,
así que las migraciones pasan sin problema). El login **no** puede autocrearse desde la app; eso lo hago yo
una vez.

**14. Convención de nombres de BD**
Las BDs existentes son: `ICARUSDB`, `ModasSussyDB`, `ClinicaSueciaDB`, `PanelInvoiceDB`, `MEDICAMENTOSDB`,
`VitiligoCenter`. El patrón dominante es **PascalCase + sufijo `DB`**. El repo pide `DecorationsDb`, que se
sale del patrón. **Recomiendo nombrarla `DecoracionesDB`** para respetar la convención y evitar confusión.
Es indiferente para la app siempre que la cadena de conexión (`Database=…`) coincida. Dime si prefieres
mantener `DecorationsDb`; en tal caso lo respeto pero lo dejo señalado.

**15. Backup automático de SQL**
Sí, por cron:
```
0 2 * * * /var/apps/backup_sql.sh                      # respalda PanelInvoiceDB (retención 7 días)
0 6 * * * /var/apps/icarus/backups/backup_icarusdb.sh  # respalda ICARUSDB (02:00 Bolivia)
```
**Importante:** `backup_sql.sh` está **fijado a una sola BD (`PanelInvoiceDB`)** y `backup_icarusdb.sh` a
`ICARUSDB`. **La nueva BD NO entraría en el backup automáticamente.** Puedo crear un
`/var/apps/decoraciones/backups/backup_decoracionesdb.sh` (copiando el patrón) y añadir su línea al cron.
Los `.bak` se guardan en `/var/apps/sqlserver-backups` (mismo disco de la VPS). Nota: Express **no** soporta
`WITH COMPRESSION`.

---

## Bloque 3 — Red Docker y conectividad

**16. ¿Unirse a red existente o crear propia?**
**Unirse a la red existente `trajano-shared-network`** (como red `external` en el compose, igual que
icarus). Así la app ve a SQL Server por nombre sin publicar nada raro. **No crear red propia.**

**17. ¿nginx y SQL en la misma red?**
**nginx NO es contenedor** — corre en el **host** (ver Bloque 4), así que no está en ninguna red Docker.
Alcanza a las apps por el **puerto publicado en `127.0.0.1`**. SQL Server sí está en
`trajano-shared-network`. La app decoraciones estará en `trajano-shared-network` (habla con SQL por ahí) y
además publicará un puerto en el host para que nginx la alcance. Cubre ambos caminos.

**18. Redes Docker existentes**
```
NETWORK ID     NAME                     DRIVER    SCOPE
76e5c58defe6   bridge                   bridge    local
ad7891195293   host                     host      local
42b3b330270c   icarus_icarus-network    bridge    local   (vacía, resto de un compose viejo)
cc8dd479df09   none                     null      local
f49de0145090   trajano-shared-network   bridge    local   ← usar esta
```
Contenedores en `trajano-shared-network`: `trajano-sqlserver` (.2), `icarus-api` (.3), `clinicasuecia` (.4),
`icarus-web` (.5), `modassussy` (.6), `vitiligoenter` (.7), `argos` (.8).

---

## Bloque 4 — Proxy nginx y dominio/SSL

**19. ¿nginx contenedor o host?**
**Instalado en el host** (`/usr/sbin/nginx`, nginx/1.24.0 Ubuntu), escuchando en `:80` y `:443`. No es
contenedor.

**20. Gestión de virtual hosts**
Clásico **`sites-available` / `sites-enabled`** (un archivo por sitio). **No** hay nginx-proxy ni Traefik ni
labels. Ejemplos en `/etc/nginx/sites-available/`: `icarus.trajano.online`, `modassussy.trajano.online`,
`facturas.trajano.online`, etc. Crearé `/etc/nginx/sites-available/decoraciones.trajano.online` y lo
enlazaré en `sites-enabled/`.

**21. Server block de ejemplo (icarus web — el más parecido a decoraciones)**
`/etc/nginx/sites-available/icarus.trajano.online`:
```nginx
server {
    listen 80;
    server_name icarus.trajano.online;
    return 301 https://$server_name$request_uri;
}
server {
    listen 443 ssl http2;
    server_name icarus.trajano.online;

    ssl_certificate     /etc/letsencrypt/live/icarus.trajano.online/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/icarus.trajano.online/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:10m;
    ssl_stapling on; ssl_stapling_verify on;
    ssl_trusted_certificate /etc/letsencrypt/live/icarus.trajano.online/chain.pem;

    add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    add_header X-Content-Type-Options nosniff always;
    add_header X-Frame-Options DENY always;

    access_log /var/log/nginx/icarus_web_access.log;
    error_log  /var/log/nginx/icarus_web_error.log;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_read_timeout 300s; proxy_send_timeout 300s; proxy_connect_timeout 300s;
        client_max_body_size 200M;
    }
}
```
Para decoraciones copiaré este patrón cambiando `server_name`, rutas de cert/log, `proxy_pass` al puerto de
decoraciones y `client_max_body_size` a `12M`.

**22. SSL/TLS**
**Let's Encrypt / certbot.** Certificados actuales en `/etc/letsencrypt/live/`: `api.icarus.trajano.online`,
`argos.icarus.trajano.online`, `icarus.trajano.online`, `api.facturas.trajano.online`, `vitiligocenter.org`.
**El certificado de `decoraciones.trajano.online` NO existe todavía → hay que emitirlo.** El proceso es
**manual pero simple** (no hay companion automático porque nginx es del host): una vez el server block esté
puesto y el DNS resuelva, corro
`certbot --nginx -d decoraciones.trajano.online` (o `certonly` + editar el block). Renovación automática ya
está por cron. (Hay además un wildcard manual `*.trajano.online` en `/etc/ssl/trajano/` como plan B.)

**23. DNS en IONOS**
✅ **Ya resuelve y apunta al VPS.** Verificado:
```
decoraciones.trajano.online → 194.164.171.217   (= IP del VPS)
```
Resuelve a un registro A a la IP del servidor. Todo listo por el lado DNS; podemos emitir el cert cuando
quieras.

**24. `client_max_body_size` para subidas de 8 MB**
Cada sitio lo define en su propio block (no hay valor global; icarus web usa `200M`). El server block nuevo
de decoraciones **hay que crearlo con `client_max_body_size 12M`** (como recomiendas) para las fotos de 8 MB
con margen. Lo dejo puesto yo al crear el vhost.

**25. Puerto del host para publicar la app**
nginx es del host, así que **sí hay que publicar un puerto** y nginx hace `proxy_pass http://127.0.0.1:<puerto>`.
Puertos **ya ocupados**: `1433, 8080 (icarus-web), 8081 (icarus-api), 8002 (clinicasuecia), 8004 (modassussy),
8005 (vitiligoenter)`, más 80/443 (nginx). Para evitar colisión propongo publicar
**`8082:8080`** (host 8082 → contenedor 8080). Queda contiguo a la serie .NET (8080/8081) y libre. El
`proxy_pass` de nginx apuntaría a `http://127.0.0.1:8082`. Si prefieres otro (8003, 8006…), dime.

---

## Bloque 5 — GitHub Actions / CI-CD

**26. ¿Cómo llega el código al VPS?**
Opción **(a): el runner de GitHub hace SSH al VPS**. Verificado: existe la clave de deploy
`~/.ssh/github_actions_key` (ed25519) y su `.pub` **está en `~/.ssh/authorized_keys`** con el comentario
`github-actions-icarus`. El workflow de icarus compila con `dotnet publish`, hace `rsync` de los binarios al
VPS y por SSH reconstruye la imagen + `docker compose up -d`. **No hay registry ni self-hosted runner.**
Replicaremos esto para decoraciones.

**27. Registry**
**No se usa registry.** Las imágenes se construyen **en el propio VPS** (`docker build` sobre los binarios
publicados que llegan por rsync). Nada que autenticar contra GHCR/Docker Hub.

**28. Secrets SSH en GitHub (deploy)**
El repo de icarus usa estos nombres (documentados en su workflow), y sirven igual para decoraciones porque es
el mismo VPS y la misma clave:
- `VPS_HOST` = `194.164.171.217`
- `VPS_USER` = `root`
- `VPS_SSH_KEY` = contenido de la **clave privada** `~/.ssh/github_actions_key` (ya autorizada en el VPS).

Para decoraciones **reutilizaremos la misma clave** (ya está en `authorized_keys`) o, si prefieres aislar,
genero una clave nueva dedicada `github_actions_decoraciones` y la añado a `authorized_keys`. Recomiendo una
clave dedicada por app (más fácil de revocar). Dime tu preferencia.

**29. Workflow `.yml` de ejemplo (icarus, adaptable a decoraciones)**
```yaml
name: Deploy to Production
on:
  push:
    branches: [ main, master ]
  workflow_dispatch:
jobs:
  deploy-web:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.0.x' }
      - run: dotnet restore Decoraciones/Decoraciones.csproj
      - run: dotnet publish Decoraciones/Decoraciones.csproj -c Release -o ./publish/web
      - uses: webfactory/ssh-agent@v0.9.0
        with: { ssh-private-key: ${{ secrets.VPS_SSH_KEY }} }
      - run: |
          mkdir -p ~/.ssh
          ssh-keyscan -H ${{ secrets.VPS_HOST }} >> ~/.ssh/known_hosts
      - run: rsync -avz --delete ./publish/web/ ${{ secrets.VPS_USER }}@${{ secrets.VPS_HOST }}:/var/apps/decoraciones/web/
      - run: |
          ssh ${{ secrets.VPS_USER }}@${{ secrets.VPS_HOST }} << 'ENDSSH'
            cd /var/apps/decoraciones
            docker build --no-cache -f Dockerfile.web -t decoraciones:latest .
            docker compose up -d decoraciones
          ENDSSH
```
> Nota: icarus usa `docker-compose` (v1) en su workflow; para decoraciones usaré `docker compose` (v2).
> Ajustaremos nombres de proyecto/DLL cuando vea la estructura real del repo (`.csproj`, nombre del `.dll`).

**30. Rama que dispara el deploy**
En icarus: `push` a **`main`/`master`** + `workflow_dispatch` manual (con filtros por mensaje de commit tipo
`[deploy-web]`). Para decoraciones propongo lo simple: **push a `main` + `workflow_dispatch`**, sin filtros de
commit al principio. Dime si quieres el patrón de tags/mensajes.

**31. Recompilación de Tailwind CSS**
El Dockerfile de referencia (icarus) **no** corre Node/npm: solo copia binarios ya publicados y hace
`ENTRYPOINT dotnet …`. Coherente con tu nota (el `output.css` ya viene en el repo). **Recomiendo commitear
`output.css`** y que el pipeline haga solo `dotnet publish` + build de imagen. Si en el futuro quieres
regenerar CSS en CI, se añade un step de Node antes del publish, pero por ahora no hace falta y mantiene el
pipeline igual al de las otras apps.

---

## Bloque 6 — Variables de entorno y secretos

**32. Gestión de secretos en el VPS**
Patrón actual: variables **en el propio `docker-compose.yml`** (icarus mete `ASPNETCORE_ENVIRONMENT`, puertos,
etc. inline; la contraseña de SQL está inline en el compose de infrastructure — no ideal). **Para decoraciones
recomiendo mejorar esto: un archivo `/var/apps/decoraciones/.env` NO versionado** (permisos `600`, root) con
`ConnectionStrings__DefaultConnection` y credenciales, referenciado desde el compose con `env_file:`. Así el
secreto no viaja en el repo ni en el rsync. Docker secrets sería overkill (no hay Swarm). El `.env` lo creo yo
en el host con los valores que me pases por canal seguro.

**33. `ASPNETCORE_ENVIRONMENT=Production`**
Sí, todas las apps corren en **`Production`** (icarus-web e icarus-api lo tienen). ⚠️ Como avisas, si el seed
del admin **solo corre en `Development`**, en `Production` **no se creará el admin** y no podrás entrar la
primera vez. Opciones (recomiendo la **A**):
- **A (recomendada):** ajustar el arranque para que el seed del admin corra también en `Production` **una sola
  vez si no existe admin** (idempotente: `if (!users.Any()) seed()`), leyendo las credenciales del admin desde
  variables de entorno. Es lo más limpio y no expone credenciales en código.
- **B:** arrancar la primera vez con `ASPNETCORE_ENVIRONMENT=Development` solo para sembrar, luego cambiar a
  `Production`. Rápido pero feo y propenso a olvidos.
- **C:** insertar el admin por SQL a mano tras aplicar migraciones.

Necesito que el **agente local ajuste el código** para la opción A (o me confirmes B/C). Dime cuál.

**34. Credenciales del admin en producción**
La semilla por defecto `admin@decoraciones.com` / `Admin@123456` **no debe usarse en producción**. Pásame por
canal seguro el **email y contraseña definitivos**; los inyecto como variables de entorno
(`SeedAdmin__Email` / `SeedAdmin__Password` o como decida el código en la opción A del punto 33) en el `.env`
del host. No los pongas en el repo.

---

## Bloque 7 — Persistencia, volúmenes y logs

**35. Volumen de uploads (`/app/wwwroot/uploads`)**
icarus usa **bind mounts** a `/var/apps/icarus/api-data/...` (no volúmenes con nombre). **Recomiendo seguir el
mismo patrón**: bind mount **`/var/apps/decoraciones/uploads → /app/wwwroot/uploads`**. Ventajas: entra fácil
en backups, se inspecciona directo desde el host y el rsync del deploy (que borra `web/`) no lo toca. Si
prefieres volumen con nombre `decoraciones-app-uploads`, también vale, pero el bind mount es lo consistente
con este VPS.

**36. Backup de volúmenes/carpetas**
Hoy **solo hay backup de las BDs de SQL** (por cron), **no de carpetas/volúmenes del host**. Si los uploads
son valiosos, hay que añadir su respaldo explícito. Propongo: colocarlos en `/var/apps/decoraciones/uploads`
(dentro de `/var/apps`, disco principal) y añadir un cron que haga `tar`/rsync de esa carpeta (a
`/var/apps/sqlserver-backups` o a donde definas). Sin esto, los uploads **no se respaldan solos**. Dime si
quieres que monte ese cron.

**37. Logs de la app (`/app/logs`, Serilog diario)**
icarus monta `Logs/` por bind a `/var/apps/icarus/*/Logs`. Recomiendo igual para decoraciones: bind mount
**`/var/apps/decoraciones/logs → /app/logs`** para tenerlos en el host. **No hay rotación/limpieza
centralizada de logs de app** (Serilog ya hace archivo diario; nginx sí tiene logrotate ~14 días). Si quieres,
añado un logrotate o un cron de limpieza para `/var/apps/decoraciones/logs`. Con `docker compose logs` solo no
bastaría a largo plazo (se pierden al recrear el contenedor), por eso el bind mount.

---

## Bloque 8 — Operación y varios

**38. Política de reinicio**
icarus usa `restart: always`; infrastructure (SQL) usa `restart: unless-stopped`. Tu compose propone
`unless-stopped`, que es una buena opción. **Recomiendo `unless-stopped`** para decoraciones (no revive si lo
paras tú a propósito). Cualquiera de los dos es coherente con el VPS.

**39. Monitorización / healthchecks**
**No hay** stack centralizado: ni Uptime Kuma, ni Portainer, ni Prometheus/Grafana, ni netdata. Solo
`docker stats`/`docker logs` puntuales y `sysstat` (sar) a nivel SO. icarus-web/api **no** exponen healthcheck;
solo `argos` tiene healthcheck de Docker. No hay dónde "registrar" el servicio. Si quieres, puedo añadir un
`healthcheck:` de Docker al compose de decoraciones (curl al `/` o a un `/health`), que al menos marca el
contenedor como healthy/unhealthy. Alertas automáticas siguen sin existir (las caídas se detectan a mano).

**40. Convención de nombres de contenedor**
Nombres actuales: `icarus-web`, `icarus-api`, `trajano-sqlserver`, `clinicasuecia`, `modassussy`,
`vitiligoenter`, `argos`. Patrón: nombre de app en minúsculas (con sufijo de rol si hay varios servicios).
Tu propuesta **`decoraciones-app`** encaja. Como decoraciones es un único servicio, también valdría solo
**`decoraciones`** (estilo `modassussy`/`clinicasuecia`). Cualquiera sirve; **recomiendo `decoraciones`**
(más corto, coincide con dominio/carpeta) salvo que prevean un `decoraciones-api` aparte, en cuyo caso mejor
`decoraciones-web` / `decoraciones-api`.

**41. Restricción de recursos**
Hoy **ningún** contenedor declara límites de CPU/memoria (ni siquiera SQL Server). El VPS tiene **4 vCPU** y
**7.7 GiB RAM** (ahora mismo ~3.3 GiB usados, **swap 8 GB con ~1 GB ya en uso** → la RAM está algo justa).
**Recomiendo poner límites suaves** a decoraciones para no competir con SQL, p. ej.:
```yaml
deploy:
  resources:
    limits: { cpus: '1.0', memory: 512M }
```
(o `mem_limit: 512m` / `cpus: 1.0` en sintaxis clásica). No es obligatorio, pero con la RAM justa es sano.

**42. Otras cosas del entorno**
- **Firewall:** `ufw` NO instalado; el filtrado real lo hace **iptables** (política `INPUT` en ACCEPT, permisiva)
  + **fail2ban** (`f2b-nginx-limit-req` en 80/443, `f2b-sshd` en 22). El 1433 está protegido por reglas en
  `DOCKER-USER` (solo VPN). El puerto que publiquemos para decoraciones (8082) quedará accesible en el host;
  el acceso público real va por nginx (443). Si quieres, puedo añadir una regla `DOCKER-USER` para que 8082
  no sea alcanzable desde fuera (solo `127.0.0.1`/nginx), como higiene.
- **WireGuard:** activo (`wg0`, `10.8.0.1/24`, `51820/udp`) — necesario para administrar SQL desde fuera.
- **Zona horaria del servidor:** **UTC** (`Etc/UTC`). icarus fija `TZ=America/La_Paz` en su contenedor API.
  Si decoraciones muestra/loguea horas para Bolivia, conviene poner **`TZ=America/La_Paz`** en su contenedor.
- **SQL Express:** recordatorio del tope de 10 GB/BD y RAM limitada; para decoraciones no es problema hoy.
- **Higiene pendiente heredada:** la contraseña de `sa` (`Password123!`) sigue sin rotar. Por eso insisto en
  crear un usuario dedicado para decoraciones en vez de reutilizar `sa`.

---

## Resumen de lo que haré una vez confirmes (checklist)

1. Crear `/var/apps/decoraciones/` (con `web/`, `uploads/`, `logs/`, `backups/`).
2. En SQL: crear BD **`DecoracionesDB`** + login/usuario dedicado **`decoraciones_app`** (`db_owner` de esa BD).
   → **Necesito la contraseña del usuario por canal seguro.**
3. Adaptar `docker-compose.yml`: red `external` `trajano-shared-network`, `env_file: .env`, publicar
   **`8082:8080`**, binds de `uploads`/`logs`, `restart: unless-stopped`, `TZ=America/La_Paz`, límites de
   recursos, (opcional) `healthcheck`.
4. Crear `.env` (600) con `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection` (usuario
   dedicado) y credenciales del admin. → **Necesito credenciales del admin por canal seguro.**
5. Confirmar con el agente local el **ajuste del seed del admin para Production** (opción A del punto 33).
6. Crear el vhost nginx `decoraciones.trajano.online` (`proxy_pass 127.0.0.1:8082`, `client_max_body_size 12M`)
   y emitir cert Let's Encrypt.
7. Añadir el workflow de GitHub Actions (SSH deploy) + secrets `VPS_HOST`/`VPS_USER`/`VPS_SSH_KEY` (decidir si
   clave nueva dedicada).
8. Añadir cron de backup de `DecoracionesDB` (y opcionalmente de `uploads/`).

### Decisiones que necesito de ti / del agente local
- [ ] Usuario dedicado de BD (recomendado) vs `sa`, y contraseña por canal seguro.
- [ ] Nombre de BD: `DecoracionesDB` (recomendado) vs `DecorationsDb`.
- [ ] Puerto host: `8082` (propuesto) u otro.
- [ ] Nombre de contenedor: `decoraciones` (recomendado) vs `decoraciones-app`.
- [ ] Clave SSH: reutilizar la de icarus vs clave dedicada nueva.
- [ ] Ajuste del seed del admin en Production (opción A/B/C) — requiere cambio de código local.
- [ ] Credenciales del admin de producción por canal seguro.
- [ ] ¿Añado backup de `uploads/` y logrotate de logs de app?
