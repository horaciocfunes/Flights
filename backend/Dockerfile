# Stage 1: Build
# sdk:8.0 (~750 MB) — compiles and publishes; never ships to production.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first so dotnet restore is cached separately from source changes.
COPY src/SkyRoute.Domain/SkyRoute.Domain.csproj         src/SkyRoute.Domain/
COPY src/SkyRoute.Application/SkyRoute.Application.csproj src/SkyRoute.Application/
COPY src/SkyRoute.Infrastructure/SkyRoute.Infrastructure.csproj src/SkyRoute.Infrastructure/
COPY src/SkyRoute.API/SkyRoute.API.csproj               src/SkyRoute.API/

RUN dotnet restore src/SkyRoute.API/SkyRoute.API.csproj

COPY . .
RUN dotnet publish src/SkyRoute.API/SkyRoute.API.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
# aspnet:8.0 (~200 MB) — no SDK, no source, no NuGet cache.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SkyRoute.API.dll"]
