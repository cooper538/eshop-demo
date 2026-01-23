# Task 3: EShop.Contracts

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01, task-02 |

## Summary
Implementovat integration events a shared DTOs pro cross-service komunikaci.

## Scope
- [ ] Vytvořit projekt `EShop.Contracts` v `src/Common/EShop.Contracts/`
- [ ] Implementovat `Events/IntegrationEvent.cs` - base record s EventId a Timestamp
- [ ] Implementovat Order events:
  - `Events/Order/OrderConfirmedEvent.cs`
  - `Events/Order/OrderRejectedEvent.cs`
  - `Events/Order/OrderCancelledEvent.cs`
- [ ] Implementovat Product events:
  - `Events/Product/StockLowEvent.cs`
  - `Events/Product/ProductCreatedEvent.cs`
  - `Events/Product/ProductUpdatedEvent.cs`
- [ ] Vytvořit `Constants/EventNames.cs` - konstanty pro event routing
- [ ] Vytvořit základní DTOs strukturu (Order/OrderItemDto, Product/ProductDto)

## Related Specs
- → [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.2 - EShop.Contracts)

---
## Notes
(Updated during implementation)
