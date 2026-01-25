# Task 01: Clean Architecture Projects

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create the 4-project Clean Architecture structure for Order Service with proper references and NuGet packages.

## Scope
- [ ] Create Order.Domain project (class library)
- [ ] Create Order.Application project (class library, references Domain)
- [ ] Create Order.Infrastructure project (class library, references Application)
- [ ] Create Order.API project (web API, references Infrastructure)
- [ ] Add all projects to EShopDemo.sln under src/Services/Order/
- [ ] Configure NuGet packages in Directory.Packages.props:
  - MediatR
  - FluentValidation
  - Microsoft.EntityFrameworkCore
  - Npgsql.EntityFrameworkCore.PostgreSQL
  - Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
- [ ] Add project references:
  - Domain → EShop.SharedKernel
  - Application → Domain, EShop.SharedKernel, EShop.Common, EShop.Contracts
  - Infrastructure → Application, EShop.Common
  - API → Infrastructure, EShop.ServiceDefaults, EShop.Common

## Reference Implementation
See `src/Services/Products/` for identical project structure.

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 9: Project Structure)

---
## Notes
(Updated during implementation)
