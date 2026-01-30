# Task 04: CQRS Queries

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01, task-02, task-03 |

## Summary
Implement query handlers for reading product data including batch queries for internal API.

## Scope
- [x] Create GetProductsQuery with filtering (category) and pagination (page, pageSize)
- [x] Create GetProductsQueryHandler using IProductDbContext
- [x] Create GetProductsQueryValidator (FluentValidation)
- [x] Create GetProductsResult DTO with Items, Page, PageSize, TotalCount, TotalPages
- [x] Create GetProductByIdQuery(Guid id)
- [x] Create GetProductByIdQueryHandler
- [x] Create GetProductByIdQueryValidator
- [x] Throw NotFoundException when product not found
- [x] Create ProductDto for response mapping (REST API)
- [x] Create GetProductsBatchQuery(IReadOnlyList<Guid> ProductIds) for gRPC
- [x] Create GetProductsBatchQueryHandler
- [x] Create GetProductsBatchResult with ProductInfoDto list
- [x] Create ProductInfoDto (ProductId, Name, Description, Price, StockQuantity)

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 2.1: List Products)
- → [error-handling.md](../../high-level-specs/error-handling.md) (Section 6.1: Throwing Exceptions)

---
## Notes
**Query structure**:
```
Queries/
├── GetProducts/
│   ├── GetProductsQuery.cs
│   ├── GetProductsQueryHandler.cs
│   ├── GetProductsQueryValidator.cs
│   └── GetProductsResult.cs
├── GetProductById/
│   ├── GetProductByIdQuery.cs
│   ├── GetProductByIdQueryHandler.cs
│   └── GetProductByIdQueryValidator.cs
└── GetProductsBatch/
    ├── GetProductsBatchQuery.cs
    ├── GetProductsBatchQueryHandler.cs
    └── GetProductsBatchResult.cs
```

**DTOs**:
- `ProductDto` - full product details for REST API
- `ProductInfoDto` - minimal info for gRPC batch response (includes stock quantity)
