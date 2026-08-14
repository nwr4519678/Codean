#!/bin/sh
# Generic entrypoint for Platform runtime images.
# If RUN_MIGRATIONS=true, applies EF Core migrations on startup, then execs the app.
set -e

if [ "${RUN_MIGRATIONS:-false}" = "true" ] && [ -f "/app/Platform.Api.dll" ]; then
    echo "[entrypoint] applying database migrations..."
    dotnet exec /app/Platform.Api.dll --migrate
fi

exec dotnet exec "/app/${APP_DLL:-$(basename /app/*.dll | head -1)}" "$@"
