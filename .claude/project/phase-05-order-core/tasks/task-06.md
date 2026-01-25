# Task 06: CancelOrder Command

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | :white_circle: pending |
| Dependencies | task-02, task-03 |

## Summary
Implement CancelOrder command with status validation.

## Scope

### Command
- [ ] Create `CancelOrderCommand` : ICommand<CancelOrderResult>
  ```csharp
  public sealed record CancelOrderCommand(
      Guid OrderId,
      string Reason
  ) : ICommand<CancelOrderResult>;
  ```

### Result
- [ ] Create `CancelOrderResult`
  ```csharp
  public sealed record CancelOrderResult(
      Guid OrderId,
      string Status,
      bool Success,
      string? Message = null
  );
  ```

### Handler
- [ ] Create `CancelOrderCommandHandler`
  ```csharp
  public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken ct)
  {
      // 1. Fetch order
      var order = await _dbContext.Orders
          .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

      if (order is null)
          throw NotFoundException.For<OrderEntity>(request.OrderId);

      // 2. Try to cancel (validates current status)
      try
      {
          order.Cancel(request.Reason);
      }
      catch (InvalidOrderStateException ex)
      {
          return new CancelOrderResult(
              order.Id,
              order.Status.ToString(),
              Success: false,
              Message: ex.Message);
      }

      // 3. Persist
      await _dbContext.SaveChangesAsync(ct);

      // 4. Return success
      return new CancelOrderResult(
          order.Id,
          order.Status.ToString(),
          Success: true,
          Message: "Order cancelled successfully");
  }
  ```

### Validator
- [ ] Create `CancelOrderCommandValidator`
  - OrderId: NotEmpty
  - Reason: NotEmpty, MaxLength(500)

## Status Rules
- Can only cancel orders in `Confirmed` state
- Orders in `Created`, `Rejected`, `Cancelled`, `Shipped` cannot be cancelled
- `InvalidOrderStateException` is caught and returned as unsuccessful result (not thrown as 400)

## Important Notes

**Phase 5 Limitation**: No stock release
- No Product Service integration (that's phase 6)
- No `ReleaseStock` call to Product Service
- Stock release will be added in phase 6

## Reference Implementation
See lifecycle methods in `OrderEntity` (task-02) and `UpdateProductCommandHandler` for error handling patterns.

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 5.3: Cancel Order flow)

---
## Notes
(Updated during implementation)
