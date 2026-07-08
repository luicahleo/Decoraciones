#!/usr/bin/env bash
# rebuild.sh - Compila la imagen de la app y levanta los contenedores
# Uso: ./rebuild.sh            -> reconstruye y levanta solo la app
#      ./rebuild.sh --all      -> reconstruye y levanta todos los servicios
#      ./rebuild.sh --logs     -> al terminar, sigue los logs de la app

set -euo pipefail
cd "$(dirname "$0")"

BUILD_TARGET="app"
FOLLOW_LOGS=false

for arg in "$@"; do
  case "$arg" in
    --all)  BUILD_TARGET="" ;;
    --logs) FOLLOW_LOGS=true ;;
  esac
done

echo "==> Reconstruyendo y levantando contenedor(es)..."
docker compose up -d --build $BUILD_TARGET

echo "==> Contenedores en ejecución:"
docker compose ps

echo "==> App disponible en http://localhost:8080  (Admin: http://localhost:8080/Admin)"

if [ "$FOLLOW_LOGS" = true ]; then
  echo "==> Siguiendo logs de la app (Ctrl+C para salir)..."
  docker compose logs -f app
fi
