# Phase 5: Order Service Core

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Implement Order Service with domain and state machine

## Scope
- [ ] Create Clean Architecture structure
- [ ] Implement Order entity with state machine (Pending → Confirmed/Rejected → Cancelled)
- [ ] Implement OrderItem value object
- [ ] Create CQRS handlers (CreateOrder, GetOrder, CancelOrder)
- [ ] Configure EF Core with PostgreSQL
- [ ] Create external REST API endpoints

## Related Specs
- → [order-service-interface.md](../high-level-specs/order-service-interface.md)
- → [error-handling.md](../high-level-specs/error-handling.md)

---
## Notes
(Updated during implementation)
