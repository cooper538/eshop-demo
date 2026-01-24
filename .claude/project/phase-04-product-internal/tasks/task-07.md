# Task 07: Internal REST API Endpoints

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | :white_circle: pending |
| Dependencies | task-04, task-05 |

## Summary
Add HTTP fallback endpoints for internal API under /internal/* path, hidden from Swagger.

## Scope
- [ ] Create Controllers/Internal directory
- [ ] Create InternalProductsController with route "internal/products"
- [ ] Add POST /internal/products/batch endpoint
- [ ] Create InternalStockController with route "internal/stock"
- [ ] Add POST /internal/stock/reserve endpoint
- [ ] Add POST /internal/stock/release endpoint
- [ ] Mark controllers with [ApiExplorerSettings(IgnoreApi = true)]
- [ ] Create request/response DTOs for HTTP API
- [ ] Use same MediatR handlers as gRPC endpoints

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 3.4: Internal REST Endpoints)
- [internal-api-communication.md](../../high-level-specs/internal-api-communication.md) (Section 2.2: Endpoint Visibility)

---
## Notes
(Updated during implementation)
