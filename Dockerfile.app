# =============================================================================
# BlogPlatform.App — Multi-stage Dockerfile  (Blazor WebAssembly + Nginx)
# Build context: repo root
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj              src/BlogPlatform/BlogPlatform.App/
COPY src/BlogPlatform/BlogPlatform.Contracts/BlogPlatform.Contracts.csproj  src/BlogPlatform/BlogPlatform.Contracts/

RUN dotnet restore src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj

FROM restore AS publish
WORKDIR /src

COPY src/BlogPlatform/ src/BlogPlatform/

RUN dotnet publish src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM nginx:1.27-alpine AS runtime

ARG BLAZOR_API_BASE_URL=http://localhost:5000/

RUN rm -rf /usr/share/nginx/html/*

COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html
COPY src/BlogPlatform/BlogPlatform.App/nginx.conf /etc/nginx/conf.d/default.conf

RUN sed -i "s#https://YOUR_API_APP_SERVICE_URL/#${BLAZOR_API_BASE_URL}#g" \
    /usr/share/nginx/html/appsettings.Production.json

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget -qO- http://localhost:80/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
