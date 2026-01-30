# Task 06: ProductGrpcService Implementation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-01, task-04, task-05 |

## Summary
Implement gRPC server for Product Service internal API, mapping gRPC requests to MediatR commands/queries.

## Scope
- [x] Create ProductGrpcService extending ProductService.ProductServiceBase
- [x] Implement GetProducts method (maps to GetProductsBatchQuery)
- [x] Implement ReserveStock method (maps to ReserveStockCommand)
- [x] Implement ReleaseStock method (maps to ReleaseStockCommand)
- [x] Map gRPC messages to domain types correctly
- [x] Serialize decimal Price as string for precision (InvariantCulture)
- [x] Implement ATOMIC validation in GetProducts (NOT_FOUND if any missing)
- [x] Register gRPC service in Program.cs (app.MapGrpcService)

## Implementation Details

**File**: `Products.API/Grpc/ProductGrpcService.cs`

**Method Mapping**:
| gRPC Method | MediatR Handler |
|-------------|-----------------|
| GetProducts | GetProductsBatchQuery |
| ReserveStock | ReserveStockCommand |
| ReleaseStock | ReleaseStockCommand |

**ATOMIC Validation (GetProducts)**:
```csharp
var missingIds = requestedIds.Where(id => !foundIds.Contains(id)).ToList();
if (missingIds.Count > 0)
{
    throw new RpcException(new Status(StatusCode.NotFound, $"Products not found: {string.Join(", ", missingIds)}"));
}
```

**Request Validators** (GrpcValidationInterceptor):
- `GetProductsRequestValidator` - validates product_ids format
- `ReserveStockRequestValidator` - validates order_id and items
- `ReleaseStockRequestValidator` - validates order_id

**Design Decision**:
- CA1062 suppressed - gRPC framework guarantees non-null request/context
- Input validation delegated to GrpcValidationInterceptor

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.6: Server Implementation)
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section 3: Server-Side Patterns)

---
## Notes
(Updated during implementation)
