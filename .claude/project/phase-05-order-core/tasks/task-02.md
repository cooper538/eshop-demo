# Task 02: Domain Layer

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | 🔵 in_progress |
| Dependencies | task-01 |

## Summary
Implement Order aggregate with state machine and OrderItem as owned entity.

## Scope

### OrderEntity (Aggregate Root)
- [ ] Create OrderEntity inheriting from AggregateRoot (EShop.SharedKernel)
- [ ] Properties:
  - `Guid CustomerId` (required)
  - `string CustomerEmail` (required)
  - `EOrderStatus Status` (required)
  - `decimal TotalAmount` (calculated from items)
  - `string? RejectionReason`
  - `DateTime CreatedAt`
  - `DateTime? UpdatedAt`
- [ ] Private collection: `_items` (List<OrderItem>)
- [ ] Public readonly: `IReadOnlyList<OrderItem> Items => _items.AsReadOnly()`
- [ ] Private constructor for EF Core
- [ ] Factory method: `Order.Create(customerId, customerEmail, items)`
  - Sets status to `Created`
  - Calculates TotalAmount from items

### State Machine Methods
- [ ] `Confirm()` - Created → Confirmed
  - Guard: throw if not in Created state
  - Sets UpdatedAt
- [ ] `Reject(string reason)` - Created → Rejected
  - Guard: throw if not in Created state
  - Sets RejectionReason, UpdatedAt
- [ ] `Cancel(string reason)` - Confirmed → Cancelled
  - Guard: throw if not in Confirmed state
  - Sets RejectionReason, UpdatedAt

### OrderItem (Owned Entity)
- [ ] Create OrderItem class (NOT inheriting from Entity - it's owned)
- [ ] Properties:
  - `Guid ProductId`
  - `string ProductName`
  - `int Quantity`
  - `decimal UnitPrice`
- [ ] Calculated: `decimal LineTotal => Quantity * UnitPrice`
- [ ] Private constructor for EF Core
- [ ] Factory method or public constructor

### Enum
- [ ] Create `EOrderStatus` enum in Domain/Enums
  - Created, Confirmed, Rejected, Cancelled, Shipped

### Exceptions
- [ ] Create `InvalidOrderStateException` in Domain/Exceptions
  - Contains: CurrentStatus, AttemptedAction
  - Message: "Cannot {action} order in {status} state"

## State Machine Diagram
```
Created ──┬──► Confirmed ──┬──► Cancelled
          │                │
          └──► Rejected    └──► Shipped (future)
```

## Reference Implementation
See `ProductEntity` in `src/Services/Products/Products.Domain/Entities/`

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 4: Order State Machine, Section 6: Domain Model)

---
## Notes
(Updated during implementation)
