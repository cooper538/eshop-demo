# Task 06: ProductGrpcService Implementation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | :white_circle: pending |
| Dependencies | task-01, task-04, task-05 |

## Summary
Implement gRPC server for Product Service internal API, mapping gRPC requests to MediatR commands/queries.

## Scope
- [ ] Create ProductGrpcService extending ProductService.ProductServiceBase
- [ ] Implement GetProducts method (maps to GetProductsBatchQuery)
- [ ] Implement ReserveStock method (maps to ReserveStockCommand)
- [ ] Implement ReleaseStock method (maps to ReleaseStockCommand)
- [ ] Map gRPC messages to domain types correctly
- [ ] Serialize decimal Price as string for precision
- [ ] Register gRPC service in Program.cs (app.MapGrpcService)
- [ ] Add GrpcExceptionInterceptor for error handling

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.6: Server Implementation)
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section 3: Server-Side Patterns)

---
## Notes
(Updated during implementation)
