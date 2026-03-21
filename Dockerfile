# Use Alpine-based runtime (smaller, fewer vulnerabilities)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

# Install required tools using Alpine package manager
RUN apk add --no-cache \
        ca-certificates \
        curl \
        busybox-extras \
        iputils && \
    update-ca-certificates

EXPOSE 80

# Use Alpine SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
ARG USE_LOCAL_NUGET=false
WORKDIR /app

# Optionally use a local NuGet feed when explicitly enabled for local builds.
RUN --mount=type=bind,source=.,target=/context,readonly \
    if [ "$USE_LOCAL_NUGET" = "true" ] && [ -d "/context/.nuget-local" ] && [ "$(ls -A /context/.nuget-local 2>/dev/null)" ]; then \
    cp -a /context/.nuget-local /app/.nuget-local && \
    dotnet nuget add source /app/.nuget-local --name local; \
    fi

# Configure GitHub Packages authentication from BuildKit secret when available.
RUN --mount=type=secret,id=github_token,required=false \
    TOKEN="$(cat /run/secrets/github_token 2>/dev/null || true)" && \
    if [ -n "$TOKEN" ]; then \
    dotnet nuget add source --username docker --password "$TOKEN" \
    --store-password-in-clear-text \
    --name github "https://nuget.pkg.github.com/hrs-org/index.json"; \
    else \
    echo "GitHub package token not provided; restore will rely on public/local feeds."; \
    fi

# Copy solution file first
COPY ["HikingRentalStore.sln", "./"]

# Copy csproj files and restore dependencies
COPY ["HRS.API/HRS.API.csproj", "HRS.API/"]
COPY ["HRS.Domain/HRS.Domain.csproj", "HRS.Domain/"]
COPY ["HRS.Infrastructure/HRS.Infrastructure.csproj", "HRS.Infrastructure/"]
COPY ["HRS.Migrations/HRS.Migrations.csproj", "HRS.Migrations/"]
COPY ["HRS.Test/HRS.Test.csproj", "HRS.Test/"]
RUN dotnet restore "HikingRentalStore.sln"

# Copy the rest of the source code (selective copying for security)
COPY ["HRS.API/", "HRS.API/"]
COPY ["HRS.Domain/", "HRS.Domain/"]
COPY ["HRS.Infrastructure/", "HRS.Infrastructure/"]
COPY ["HRS.Migrations/", "HRS.Migrations/"]
COPY ["HRS.Test/", "HRS.Test/"]
RUN dotnet build "HikingRentalStore.sln" -c "$BUILD_CONFIGURATION" --no-restore \
    -p:TreatWarningsAsErrors=false

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HRS.API/HRS.API.csproj" -c "$BUILD_CONFIGURATION" \
    -o /app/publish /p:UseAppHost=false --no-restore --no-build

# Final stage - runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user with high UID (Alpine syntax)
RUN adduser -D -u 10002 appuser && chown -R appuser /app
USER appuser

# Set environment variable to listen on port 8080 (non-privileged port)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HRS.API.dll"]
