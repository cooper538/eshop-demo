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
- [x] Serialize decimal Price as string for precision
- [x] Register gRPC service in Program.cs (app.MapGrpcService)
- [x] Add GrpcExceptionInterceptor for error handling

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.6: Server Implementation)
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section 3: Server-Side Patterns)

---
## Notes
(Updated during implementation)
