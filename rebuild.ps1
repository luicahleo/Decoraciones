# rebuild.ps1 - Compila la imagen de la app y levanta los contenedores
# Uso: .\rebuild.ps1            -> reconstruye y levanta solo la app
#      .\rebuild.ps1 -All       -> reconstruye y levanta todos los servicios
#      .\rebuild.ps1 -Logs      -> al terminar, sigue los logs de la app

param(
    [switch]$All,
    [switch]$Logs
)

$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot

Write-Host "==> Reconstruyendo y levantando contenedor(es)..." -ForegroundColor Cyan

if ($All) {
    docker compose up -d --build
} else {
    docker compose up -d --build app
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "==> Error al construir/levantar. Revisa la salida de arriba." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "==> Contenedores en ejecución:" -ForegroundColor Green
docker compose ps

Write-Host "==> App disponible en http://localhost:8080  (Admin: http://localhost:8080/Admin)" -ForegroundColor Green

if ($Logs) {
    Write-Host "==> Siguiendo logs de la app (Ctrl+C para salir)..." -ForegroundColor Cyan
    docker compose logs -f app
}
