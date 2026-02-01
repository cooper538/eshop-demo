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
- [ ] Configure non-root user ($APP_UID) for security

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 6.2 Image Naming Convention)

---
## Notes
- Use .NET 10 base images (mcr.microsoft.com/dotnet/aspnet:10.0)
- Multi-stage build minimizes final image size
- Build context is solution root for shared project access
