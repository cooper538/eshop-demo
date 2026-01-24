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
- [ ] Create Product.Domain project (class library)
- [ ] Create Product.Application project (class library, references Domain)
- [ ] Create Product.Infrastructure project (class library, references Application)
- [ ] Create Product.API project (web API, references Infrastructure)
- [ ] Add all projects to EShopDemo.sln
- [ ] Configure NuGet packages (MediatR, FluentValidation, EF Core PostgreSQL)
- [ ] Add references to EShop.SharedKernel, EShop.Common, EShop.Contracts
- [ ] Add reference to EShop.ServiceDefaults in API project

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 9: Project Structure)

---
## Notes
(Updated during implementation)
