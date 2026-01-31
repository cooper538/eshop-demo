# Task 06: Dockerfiles

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | pending |
| Dependencies | - |

## Summary
Create optimized multi-stage Dockerfiles for all services with .NET 10 runtime and proper layer caching.

## Scope
- [ ] Create `Dockerfile` for Gateway.API
- [ ] Create `Dockerfile` for Product.API
- [ ] Create `Dockerfile` for Order.API
- [ ] Create `Dockerfile` for NotificationService
- [ ] Create `Dockerfile` for Catalog.API
- [ ] Create `Dockerfile` for DatabaseMigration job
- [ ] Create `.dockerignore` file for build optimization
- [ ] Configure non-root user for security
- [ ] Optimize layer caching with proper COPY order

## Dockerfile Template

```dockerfile
# src/Services/Product/Product.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props and Packages.props first (better caching)
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

# Copy project files
COPY ["src/Common/EShop.SharedKernel/EShop.SharedKernel.csproj", "src/Common/EShop.SharedKernel/"]
COPY ["src/Common/EShop.Contracts/EShop.Contracts.csproj", "src/Common/EShop.Contracts/"]
COPY ["src/Common/EShop.Common.Api/EShop.Common.Api.csproj", "src/Common/EShop.Common.Api/"]
COPY ["src/Common/EShop.Common.Application/EShop.Common.Application.csproj", "src/Common/EShop.Common.Application/"]
COPY ["src/Common/EShop.Common.Infrastructure/EShop.Common.Infrastructure.csproj", "src/Common/EShop.Common.Infrastructure/"]
COPY ["src/ServiceDefaults/EShop.ServiceDefaults.csproj", "src/ServiceDefaults/"]
COPY ["src/Services/Product/Product.Domain/Product.Domain.csproj", "src/Services/Product/Product.Domain/"]
COPY ["src/Services/Product/Product.Application/Product.Application.csproj", "src/Services/Product/Product.Application/"]
COPY ["src/Services/Product/Product.Infrastructure/Product.Infrastructure.csproj", "src/Services/Product/Product.Infrastructure/"]
COPY ["src/Services/Product/Product.API/Product.API.csproj", "src/Services/Product/Product.API/"]

# Restore
RUN dotnet restore "src/Services/Product/Product.API/Product.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/Product/Product.API"
RUN dotnet build "Product.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Product.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Product.API.dll"]
```

## Docker Ignore File

```
# .dockerignore
**/.dockerignore
**/.git
**/.gitignore
**/.vs
**/.vscode
**/*.*proj.user
**/bin
**/obj
**/Dockerfile*
**/docker-compose*
**/*.md
**/node_modules
**/.idea
**/TestResults
**/.coverage
```

## Services to Containerize

| Service | Dockerfile Path | Port |
|---------|-----------------|------|
| Gateway.API | src/Services/Gateway/Gateway.API/Dockerfile | 8080 |
| Product.API | src/Services/Product/Product.API/Dockerfile | 8080 |
| Order.API | src/Services/Order/Order.API/Dockerfile | 8080 |
| NotificationService | src/Services/Notification/NotificationService/Dockerfile | 8080 |
| Catalog.API | src/Services/Catalog/Catalog.API/Dockerfile | 8080 |
| DatabaseMigration | src/Services/DatabaseMigration/Dockerfile | - |

## Files to Create

| Action | File |
|--------|------|
| CREATE | `src/Services/Gateway/Gateway.API/Dockerfile` |
| CREATE | `src/Services/Product/Product.API/Dockerfile` |
| CREATE | `src/Services/Order/Order.API/Dockerfile` |
| CREATE | `src/Services/Notification/NotificationService/Dockerfile` |
| CREATE | `src/Services/Catalog/Catalog.API/Dockerfile` |
| CREATE | `src/Services/DatabaseMigration/Dockerfile` |
| CREATE | `.dockerignore` |

## Verification

```bash
# Build locally
docker build -t product-service -f src/Services/Product/Product.API/Dockerfile .

# Test run
docker run -p 8080:8080 product-service
```

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 6.2 Image Naming Convention)

---
## Notes
- Use .NET 10 base images (mcr.microsoft.com/dotnet/aspnet:10.0)
- Non-root user ($APP_UID) for security
- Multi-stage build minimizes final image size
- Build context is solution root for shared project access
