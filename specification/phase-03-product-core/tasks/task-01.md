# Task 01: Clean Architecture Projects

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create the 4-project Clean Architecture structure for Product Service with proper references and NuGet packages.

## Scope
- [x] Create Products.Domain project (class library)
- [x] Create Products.Application project (class library, references Domain)
- [x] Create Products.Infrastructure project (class library, references Application)
- [x] Create Products.API project (web API, references Infrastructure)
- [x] Add all projects to EShopDemo.sln
- [x] Configure NuGet packages (MediatR, FluentValidation, EF Core PostgreSQL, MassTransit, Grpc.AspNetCore)
- [x] Add references to EShop.SharedKernel, EShop.Common.*, EShop.Contracts, EShop.Grpc
- [x] Add reference to EShop.ServiceDefaults in API project

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 9: Project Structure)

---
## Notes
**Actual project names**: `Products.*` (plural) - e.g., `Products.Domain`, `Products.API`

**Project structure**:
```
src/Services/Products/
├── Products.Domain/
├── Products.Application/
├── Products.Infrastructure/
└── Products.API/
```

**Key NuGet packages used**:
- MediatR - CQRS pattern
- FluentValidation - command/query validation
- Npgsql.EntityFrameworkCore.PostgreSQL - PostgreSQL provider
- MassTransit - messaging with outbox pattern
- Grpc.AspNetCore - internal gRPC API
