# Phase 4: Product Service Internal API

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Add internal API layer for gRPC and stock management

## Scope
- [ ] Implement gRPC server (ProductGrpcService)
- [ ] Add StockReservation entity and logic
- [ ] Implement ReserveStock and ReleaseStock operations
- [ ] Add internal REST endpoints (`/internal/*`)
- [ ] Implement stock reservation expiration (TTL cleanup)

## Related Specs
- → [product-service-interface.md](../high-level-specs/product-service-interface.md) (stock operations)
- → [grpc-communication.md](../high-level-specs/grpc-communication.md)
- → [internal-api-communication.md](../high-level-specs/internal-api-communication.md)

---
## Notes
(Updated during implementation)
