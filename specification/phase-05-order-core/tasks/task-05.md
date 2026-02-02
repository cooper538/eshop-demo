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

  // Note: ProductName and UnitPrice are fetched from Product Service
  public sealed record CreateOrderItemDto(
      Guid ProductId,
      int Quantity
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
      // 1. Get product info from Product Service
      var productIds = request.Items.Select(i => i.ProductId).ToList();
      var productsResult = await _productServiceClient.GetProductsAsync(productIds, ct);
      var productLookup = productsResult.Products.ToDictionary(p => p.ProductId);

      // 2. Map items to domain objects (with product info from service)
      var items = request.Items.Select(i =>
      {
          var product = productLookup[i.ProductId];
          return OrderItem.Create(i.ProductId, product.Name, i.Quantity, product.Price);
      });

      // 3. Create order via factory method (status = Created)
      var order = OrderEntity.Create(
          request.CustomerId,
          request.CustomerEmail,
          items,
          _dateTimeProvider.UtcNow);

      // 4. Reserve stock, confirm/reject order, persist...
  }
  ```

### Validator
- [ ] Create `CreateOrderCommandValidator`
  - CustomerId: NotEmpty
  - CustomerEmail: NotEmpty, EmailAddress, MaxLength(320)
  - Items: NotEmpty (at least 1 item)
  - Each Item:
    - ProductId: NotEmpty
    - Quantity: GreaterThan(0)

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
**ACTUAL IMPLEMENTATION DIFFERS FROM ORIGINAL PLAN:**

The actual implementation includes Product Service integration (originally planned for Phase 6):
- `CreateOrderCommandHandler` first calls `IProductServiceClient.GetProductsAsync()` to fetch product names and prices
- Then calls `IProductServiceClient.ReserveStockAsync()` to reserve stock
- On successful reservation, order is automatically confirmed (not left in "Created" state)
- On failed reservation, throws `ValidationException` or rejects order
- Publishes `OrderConfirmedDomainEvent` which triggers `OrderConfirmedEvent` integration event via MassTransit

**Note:** `CreateOrderItemDto` only contains `ProductId` and `Quantity`. The `ProductName` and `UnitPrice` are fetched from Product Service at order creation time to ensure consistent pricing.

**Files:**
- `Order.Application/Commands/CreateOrder/CreateOrderCommand.cs`
- `Order.Application/Commands/CreateOrder/CreateOrderCommandHandler.cs`
- `Order.Application/Commands/CreateOrder/CreateOrderCommandValidator.cs`
- `Order.Application/Commands/CreateOrder/CreateOrderItemDtoValidator.cs`
- `Order.Application/Commands/CreateOrder/CreateOrderResult.cs`
