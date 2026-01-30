# Task 3: EShop.Contracts

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01, task-02 |

## Summary
Implement integration events and service client interfaces for cross-service communication.

## Scope
- [x] Create project `EShop.Contracts` in `src/Common/EShop.Contracts/`
- [x] Implement `IntegrationEvents/IntegrationEvent.cs` - base record with EventId and Timestamp
- [x] Implement Order events:
  - `IntegrationEvents/Order/OrderConfirmedEvent.cs` (OrderId, CustomerId, TotalAmount, CustomerEmail)
  - `IntegrationEvents/Order/OrderRejectedEvent.cs`
  - `IntegrationEvents/Order/OrderCancelledEvent.cs`
- [x] Implement Product events:
  - `IntegrationEvents/Product/StockLowEvent.cs`
  - `IntegrationEvents/Product/ProductCreatedEvent.cs`
  - `IntegrationEvents/Product/ProductUpdatedEvent.cs`
- [x] Implement service client interfaces:
  - `ServiceClients/Product/IProductServiceClient.cs` - protocol-agnostic interface
  - `ServiceClients/Product/ReserveStockRequest.cs`
  - `ServiceClients/Product/StockReservationResult.cs`
  - `ServiceClients/Product/ReleaseStockRequest.cs`
  - `ServiceClients/Product/StockReleaseResult.cs`

## Related Specs
- -> [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 3.2 - EShop.Contracts)

---
## Notes
- Removed Constants/EventNames.cs - MassTransit handles routing automatically via message type
- Service client interface moved here from EShop.ServiceClients for cleaner dependencies
- DTOs are embedded in events (primary constructor records), no separate DTO files needed
