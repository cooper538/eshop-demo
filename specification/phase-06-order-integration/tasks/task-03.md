# Task 03: Stock Reservation Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Integrate IProductServiceClient.ReserveStockAsync in CreateOrderCommandHandler to reserve stock when creating orders.

## Scope
- [x] Inject IProductServiceClient into CreateOrderCommandHandler
- [x] Call ReserveStockAsync with order items before creating order
- [x] Handle reservation failures (throw exception with failure reason)
- [x] Confirm order only after successful reservation

## Implementation

### CreateOrderCommandHandler Flow
```csharp
public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
{
    // 1. Create order entity (status: Pending)
    var order = request.ToEntity(_dateTimeProvider.UtcNow);

    // 2. Build reservation request from order items
    var orderItems = request.Items
        .Select(i => new OrderItemRequest(i.ProductId, i.Quantity))
        .ToList();
    var reservationRequest = new ReserveStockRequest(order.Id, orderItems);

    // 3. Call Product Service to reserve stock
    var result = await _productServiceClient.ReserveStockAsync(reservationRequest, ct);

    // 4. Handle failure
    if (!result.Success)
        throw new InvalidOperationException($"Stock reservation failed: {result.FailureReason}");

    // 5. Confirm order and persist
    order.Confirm(_dateTimeProvider.UtcNow);
    _dbContext.Orders.Add(order);

    return new CreateOrderResult(order.Id, order.Status.ToString());
}
```

### Key Files
- `src/Services/Order/Order.Application/Commands/CreateOrder/CreateOrderCommandHandler.cs`
- `src/Common/EShop.Contracts/ServiceClients/Product/ReserveStockRequest.cs`
- `src/Common/EShop.Contracts/ServiceClients/Product/StockReservationResult.cs`

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Stock Operations)
- [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section: Create Order Flow)

---
## Notes
Compensation logic for partial reservations is handled by Product Service - it reserves all or nothing.
