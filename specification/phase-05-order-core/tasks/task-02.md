# Task 02: Domain Layer

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement Order aggregate with lifecycle methods, OrderItem as owned entity, and domain events.

## Scope

### OrderEntity (Aggregate Root) - IMPLEMENTED
- [x] `OrderEntity` inheriting from `AggregateRoot` (EShop.SharedKernel)
- [x] Properties:
  - `Guid CustomerId` (required)
  - `string CustomerEmail` (required)
  - `EOrderStatus Status` (required)
  - `decimal TotalAmount` (calculated from items)
  - `string? RejectionReason`
  - `DateTime CreatedAt`
  - `DateTime? UpdatedAt`
- [x] Private collection: `_items` (List<OrderItem>)
- [x] Public readonly: `IReadOnlyList<OrderItem> Items`
- [x] Private constructor for EF Core
- [x] Factory method: `OrderEntity.Create(customerId, customerEmail, items, createdAt)`

### Lifecycle Methods - IMPLEMENTED
- [x] `Confirm(DateTime occurredAt)` - Created -> Confirmed
  - Guard: throws `InvalidOrderStateException` if not in Created state
  - Sets UpdatedAt, increments version
  - **Raises `OrderConfirmedDomainEvent`**
- [x] `Reject(string reason, DateTime occurredAt)` - Created -> Rejected
  - Guard: throws `InvalidOrderStateException` if not in Created state
  - Sets RejectionReason, UpdatedAt, increments version
  - **Raises `OrderRejectedDomainEvent`**
- [x] `Cancel(string reason, DateTime occurredAt)` - Confirmed -> Cancelled
  - Guard: throws `InvalidOrderStateException` if not in Confirmed state
  - Sets RejectionReason, UpdatedAt, increments version
  - **Raises `OrderCancelledDomainEvent`**

### OrderItem (Owned Entity) - IMPLEMENTED
- [x] `OrderItem` implementing `IOwnedEntity`
- [x] Properties:
  - `Guid ProductId`
  - `string ProductName`
  - `int Quantity`
  - `decimal UnitPrice`
- [x] Calculated: `decimal LineTotal => Quantity * UnitPrice`
- [x] Private constructor for EF Core
- [x] Factory method: `OrderItem.Create(productId, productName, quantity, unitPrice)`

### Enum - IMPLEMENTED
- [x] `EOrderStatus` enum in Domain/Enums
  - Created, Confirmed, Rejected, Cancelled, Shipped

### Exceptions - IMPLEMENTED
- [x] `InvalidOrderStateException` in Domain/Exceptions
  - Contains: `CurrentStatus`, `TargetStatus`
  - Message: "Cannot transition order from {currentStatus} to {targetStatus}"

### Domain Events - IMPLEMENTED (beyond original plan)
- [x] `OrderConfirmedDomainEvent` - raised on Confirm()
  - Contains: OrderId, CustomerId, CustomerEmail, TotalAmount, Items (OrderItemInfo list)
- [x] `OrderRejectedDomainEvent` - raised on Reject()
  - Contains: OrderId, CustomerId, CustomerEmail, Reason
- [x] `OrderCancelledDomainEvent` - raised on Cancel()
  - Contains: OrderId, CustomerId, CustomerEmail, Reason
- [x] `OrderItemInfo` - helper record for event data

## Status Transitions
```
Created ──┬──► Confirmed ──┬──► Cancelled
          │                │
          └──► Rejected    └──► Shipped (future)
```

## Actual Implementation Structure
```
Order.Domain/
├── Entities/
│   ├── OrderEntity.cs
│   └── OrderItem.cs
├── Enums/
│   └── EOrderStatus.cs
├── Events/
│   ├── OrderConfirmedDomainEvent.cs
│   ├── OrderRejectedDomainEvent.cs
│   ├── OrderCancelledDomainEvent.cs
│   └── OrderItemInfo.cs
└── Exceptions/
    └── InvalidOrderStateException.cs
```

## Reference Implementation
See `ProductEntity` in `src/Services/Products/Products.Domain/Entities/`

## Related Specs
- -> [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 4: Order Lifecycle, Section 6: Domain Model)

---
## Notes
**IMPLEMENTATION BEYOND ORIGINAL PLAN:**

Domain events and event handlers implemented ahead of schedule (originally planned for Phase 7):
- Domain events: `OrderConfirmedDomainEvent`, `OrderRejectedDomainEvent`, `OrderCancelledDomainEvent`
- Event handlers in Application layer publish integration events via MassTransit:
  - `OrderConfirmedDomainEventHandler` -> `OrderConfirmedEvent`
  - `OrderRejectedDomainEventHandler` -> `OrderRejectedEvent`
  - `OrderCancelledDomainEventHandler` -> `OrderCancelledEvent`

**Event Handler Files:**
- `Order.Application/EventHandlers/OrderConfirmedDomainEventHandler.cs`
- `Order.Application/EventHandlers/OrderRejectedDomainEventHandler.cs`
- `Order.Application/EventHandlers/OrderCancelledDomainEventHandler.cs`
