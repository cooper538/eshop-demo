# Task 06: Dockerfiles

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create optimized multi-stage Dockerfiles for all services with .NET 10 runtime and proper layer caching.

## Scope
- [x] Create `Dockerfile` for Gateway.API
- [x] Create `Dockerfile` for Products.API
- [x] Create `Dockerfile` for Order.API
- [x] Create `Dockerfile` for NotificationService
- [x] Create `Dockerfile` for Analytics service
- [x] Create `Dockerfile` for DatabaseMigration job
- [x] Create `.dockerignore` file for build optimization
- [x] Configure non-root user ($APP_UID) for security

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 6.2 Image Naming Convention)

---
## Notes
- Using Alpine images for smaller size (mcr.microsoft.com/dotnet/aspnet:10.0-alpine)
- Multi-stage build minimizes final image size
- Build context is solution root for shared project access
- Analytics service included; Catalog service not implemented
