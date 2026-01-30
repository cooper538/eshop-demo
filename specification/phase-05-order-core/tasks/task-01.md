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
- [x] Create Order.Domain project (class library)
- [x] Create Order.Application project (class library, references Domain)
- [x] Create Order.Infrastructure project (class library, references Application)
- [x] Create Order.API project (web API, references Infrastructure)
- [x] Add all projects to EShopDemo.sln under src/Services/Order/
- [x] Configure NuGet packages in Directory.Packages.props
- [x] Add project references

## Actual Implementation Structure
```
src/Services/Order/
├── Order.Domain/
│   └── Order.Domain.csproj
├── Order.Application/
│   └── Order.Application.csproj
├── Order.Infrastructure/
│   └── Order.Infrastructure.csproj
└── Order.API/
    └── Order.API.csproj
```

## Project References
- Domain -> EShop.SharedKernel
- Application -> Domain, EShop.SharedKernel, EShop.Common.Application, EShop.Contracts
- Infrastructure -> Application, EShop.Common.Infrastructure
- API -> Infrastructure, EShop.ServiceDefaults, EShop.Common.Api, EShop.ServiceClients

## Reference Implementation
See `src/Services/Products/` for identical project structure.

## Related Specs
- -> [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 9: Project Structure)

---
## Notes
(Updated during implementation)
