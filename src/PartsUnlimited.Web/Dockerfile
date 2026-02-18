# syntax=docker/dockerfile:1

# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore NuGet packages (cached layer)
COPY ["src/PartsUnlimited.Web/PartsUnlimited.Web.csproj", "src/PartsUnlimited.Web/"]
RUN dotnet restore "src/PartsUnlimited.Web/PartsUnlimited.Web.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/PartsUnlimited.Web"
RUN dotnet publish "PartsUnlimited.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Non-root user for least privilege
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

WORKDIR /app
COPY --from=build /app/publish .

# Health-check (container-level; AKS liveness also configured in Helm)
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "PartsUnlimited.Web.dll"]
