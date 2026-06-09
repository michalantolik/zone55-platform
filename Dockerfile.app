# =============================================================================
# BlogPlatform.App — Multi-stage Dockerfile  (Blazor WebAssembly + Nginx)
# Build context: repo root  (docker build -f Dockerfile.app .)
#
# Blazor WebAssembly compiles to static files (HTML, JS, WASM, CSS).
# The .NET SDK is only needed at build time; the final image is a lean
# Nginx container with zero .NET runtime dependency.
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1 — restore
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj         src/BlogPlatform/BlogPlatform.App/
COPY src/BlogPlatform/BlogPlatform.Contracts/BlogPlatform.Contracts.csproj  src/BlogPlatform/BlogPlatform.Contracts/

RUN dotnet restore src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj

# -----------------------------------------------------------------------------
# Stage 2 — build & publish
#   dotnet publish for Blazor WASM outputs to wwwroot inside the publish dir.
# -----------------------------------------------------------------------------
FROM restore AS publish
WORKDIR /src

COPY src/BlogPlatform/ src/BlogPlatform/

RUN dotnet publish src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# -----------------------------------------------------------------------------
# Stage 3 — runtime  (Nginx — no .NET runtime needed)
#   Serve the compiled WASM bundle as static files.
#   A custom nginx.conf enables proper MIME types for .wasm files and
#   configures a catch-all rewrite so Blazor client-side routing works.
# -----------------------------------------------------------------------------
FROM nginx:1.27-alpine AS runtime

# Remove the default Nginx placeholder page
RUN rm -rf /usr/share/nginx/html/*

# Copy compiled Blazor WASM output
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html

# Copy custom Nginx configuration
COPY src/BlogPlatform/BlogPlatform.App/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget -qO- http://localhost:80/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
