# Task 05: GetProductsBatch Query

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Implement batch product query for internal API - returns product info for multiple product IDs.

## Scope
- [x] Create GetProductsBatchQuery record with list of product IDs
- [x] Create GetProductsBatchResult record with list of ProductInfoDto
- [x] Create ProductInfoDto (ProductId, Name, Description, Price, StockQuantity)
- [x] Implement GetProductsBatchQueryHandler
- [x] Join Products with Stocks to get stock quantity
- [x] Return only products that exist (caller validates completeness)

## Implementation Details

**Files**:
- `Products.Application/Queries/GetProductsBatch/GetProductsBatchQuery.cs`
- `Products.Application/Queries/GetProductsBatch/GetProductsBatchQueryHandler.cs`
- `Products.Application/Queries/GetProductsBatch/GetProductsBatchResult.cs`
- `Products.Application/Dtos/ProductInfoDto.cs`

**Query Signature**:
```csharp
GetProductsBatchQuery(IReadOnlyList<Guid> ProductIds) : IQuery<GetProductsBatchResult>
```

**DTO**:
```csharp
ProductInfoDto(Guid ProductId, string Name, string Description, decimal Price, int StockQuantity)
```

**Design Decision**:
- Query handler returns only found products (partial result)
- ATOMIC validation happens in gRPC/HTTP layer (compares requested vs returned IDs)
- Uses JOIN to get total stock quantity from Stock table

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.1: GetProducts, Section 3.5: ServiceClients Models)

---
## Notes
(Updated during implementation)
