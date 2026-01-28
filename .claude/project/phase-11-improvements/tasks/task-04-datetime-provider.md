# Task 4: IDateTimeProvider Abstraction

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Introduce `IDateTimeProvider` abstraction for testability of time-dependent code.

## Scope
- [x] Create `IDateTimeProvider` interface in SharedKernel
- [x] Implement `DateTimeProvider` in EShop.Common
- [x] Add DI extension method `AddDateTimeProvider`
- [x] Replace `DateTime.UtcNow` usages across codebase with `IDateTimeProvider`

## Implementation Details

### Created Files
- `src/Common/EShop.SharedKernel/Services/IDateTimeProvider.cs`
- `src/Common/EShop.Common/Services/DateTimeProvider.cs`

### Modified Files

**Base Event Classes:**
- `DomainEventBase.cs` - `required DateTime OccurredOn { get; init; }`
- `IntegrationEvent.cs` - `required DateTime Timestamp { get; init; }`

**Domain Entities (added semantic DateTime parameters):**
- `OrderEntity.Create(..., DateTime createdAt)`
- `OrderEntity.Confirm(DateTime occurredAt)`
- `OrderEntity.Reject(string reason, DateTime occurredAt)`
- `OrderEntity.Cancel(string reason, DateTime occurredAt)`
- `ProductEntity.Create(..., DateTime createdAt)`
- `ProductEntity.Update(..., DateTime updatedAt)`
- `StockEntity.ReserveStock(..., DateTime reservedAt)`
- `StockReservationEntity.Create(..., DateTime reservedAt)`

**Command Handlers (injected IDateTimeProvider):**
- `CreateOrderCommandHandler`
- `CancelOrderCommandHandler`
- `CreateProductCommandHandler`
- `UpdateProductCommandHandler`
- `ReserveStockCommandHandler`
- `ExpireReservationsCommandHandler`

**Event Handlers (injected IDateTimeProvider for integration events):**
- `OrderConfirmedDomainEventHandler`
- `OrderRejectedDomainEventHandler`
- `OrderCancelledDomainEventHandler`
- `LowStockWarningEventHandler`

**Notification Service:**
- `IdempotentConsumer` base class
- All consumers (OrderConfirmed, OrderRejected, OrderCancelled, StockLow)

**DI Registration:**
- `ServiceCollectionExtensions.AddDateTimeProvider()`
- Registered in Order.API, Products.API, Notification

### Intentionally NOT Changed
- `GrpcProductServiceClient.cs` - gRPC deadline is infrastructure, not business logic

---
## Notes
- Parameter names use semantic naming: `createdAt`, `updatedAt`, `occurredAt`, `reservedAt`
- `StockEntity.ExpireStaleReservations(DateTime now)` keeps `now` as it's a reference time for comparison
