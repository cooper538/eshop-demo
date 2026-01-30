# Task 06: FluentValidation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-01, task-05 |

## Summary
Add FluentValidation validators for commands, queries, and gRPC requests.

## Scope
- [x] Create CreateProductCommandValidator
  - Name: required, max 200 chars
  - Description: required, max 2000 chars
  - Price: greater than 0
  - StockQuantity: >= 0
  - LowStockThreshold: >= 0
  - Category: required, max 100 chars
- [x] Create UpdateProductCommandValidator (same rules + Id required)
- [x] Create ReserveStockCommandValidator
- [x] Create GetProductsQueryValidator
- [x] Create GetProductByIdQueryValidator
- [x] Create gRPC validators (ReserveStockRequestValidator, ReleaseStockRequestValidator, GetProductsRequestValidator)
- [x] Register validators in DI via AddFluentValidation() from EShop.Common
- [x] ValidationBehavior pipeline integrated from EShop.Common.Application

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2.2: Create Product - field constraints)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 2.2: Validation Errors)

---
## Notes
**Validator locations**:
- Application layer: Command/Query validators
- API layer: gRPC request validators (Products.API/Grpc/Validators/)

**gRPC validators**:
```
Grpc/Validators/
├── ReserveStockRequestValidator.cs
├── ReleaseStockRequestValidator.cs
└── GetProductsRequestValidator.cs
```

**Registration**:
```csharp
// Infrastructure DI
builder.Services.AddFluentValidation(typeof(Products.Application.DependencyInjection).Assembly);

// API DI (for gRPC validators)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

**gRPC validation** uses `GrpcValidationInterceptor` from EShop.Common.Api.Grpc
