# Task 05: CreateOrder Command

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-02, task-03 |

## Summary
Implement CreateOrder command that creates order in "Created" state.

## Scope

### Command
- [ ] Create `CreateOrderCommand` : ICommand<CreateOrderResult>
  ```csharp
  public sealed record CreateOrderCommand(
      Guid CustomerId,
      string CustomerEmail,
      IReadOnlyList<CreateOrderItemDto> Items
  ) : ICommand<CreateOrderResult>;

  public sealed record CreateOrderItemDto(
      Guid ProductId,
      string ProductName,
      int Quantity,
      decimal UnitPrice
  );
  ```

### Result
- [ ] Create `CreateOrderResult`
  ```csharp
  public sealed record CreateOrderResult(
      Guid OrderId,
      string Status,
      string? Message = null
  );
  ```

### Handler
- [ ] Create `CreateOrderCommandHandler`
  ```csharp
  public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
  {
      // 1. Map items to domain objects
      var items = request.Items.Select(i => OrderItem.Create(
          i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList();

      // 2. Create order via factory method (status = Created)
      var order = OrderEntity.Create(
          request.CustomerId,
          request.CustomerEmail,
          items);

      // 3. Persist
      _dbContext.Orders.Add(order);
      await _dbContext.SaveChangesAsync(ct);

      // 4. Return result
      return new CreateOrderResult(order.Id, order.Status.ToString());
  }
  ```

### Validator
- [ ] Create `CreateOrderCommandValidator`
  - CustomerId: NotEmpty
  - CustomerEmail: NotEmpty, EmailAddress, MaxLength(320)
  - Items: NotEmpty (at least 1 item)
  - Each Item:
    - ProductId: NotEmpty
    - ProductName: NotEmpty, MaxLength(200)
    - Quantity: GreaterThan(0)
    - UnitPrice: GreaterThanOrEqualTo(0)

## Important Notes

**Phase 5 Limitation**: Order remains in "Created" state
- No Product Service integration (that's phase 6)
- No stock reservation call
- No automatic Confirm/Reject based on stock

**For testing in phase 5**: Manually call Confirm()/Reject() via separate endpoint or direct DB update.

## Reference Implementation
See `CreateProductCommand` in Products.Application

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 3.1: Create Order, Section 7.5: Handler Example)

---
## Notes
(Updated during implementation)
