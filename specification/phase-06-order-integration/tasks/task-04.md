# Task 04: Stock Release Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-03 |

## Summary
Integrate IProductServiceClient.ReleaseStockAsync in CancelOrderCommandHandler to release reserved stock when cancelling orders.

## Scope
- [x] Inject IProductServiceClient into CancelOrderCommandHandler
- [x] Call ReleaseStockAsync when order is cancelled
- [x] Handle release failures gracefully (log warning, don't fail cancellation)
- [x] Implement idempotency (release by orderId, Product Service handles duplicates)

## Implementation

### CancelOrderCommandHandler Flow
```csharp
public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken ct)
{
    var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

    if (order is null)
        throw NotFoundException.For<OrderEntity>(request.OrderId);

    try
    {
        order.Cancel(request.Reason, _dateTimeProvider.UtcNow);
    }
    catch (InvalidOrderStateException ex)
    {
        return new CancelOrderResult(order.Id, order.Status.ToString(), Success: false, Message: ex.Message);
    }

    // Release stock - graceful degradation (don't fail cancellation)
    await ReleaseStockAsync(order.Id, ct);

    return new CancelOrderResult(order.Id, order.Status.ToString(), Success: true);
}

private async Task ReleaseStockAsync(Guid orderId, CancellationToken ct)
{
    try
    {
        var result = await _productServiceClient.ReleaseStockAsync(new ReleaseStockRequest(orderId), ct);
        if (!result.Success)
            _logger.LogWarning("Failed to release stock for order {OrderId}: {Reason}", orderId, result.FailureReason);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error releasing stock for order {OrderId}", orderId);
    }
}
```

### Key Design Decisions
- **Graceful degradation**: Stock release failure doesn't fail order cancellation
- **Idempotency**: Uses orderId for release - Product Service handles duplicate releases
- **Logging**: Warnings for business failures, errors for exceptions

### Key Files
- `src/Services/Order/Order.Application/Commands/CancelOrder/CancelOrderCommandHandler.cs`
- `src/Common/EShop.Contracts/ServiceClients/Product/ReleaseStockRequest.cs`
- `src/Common/EShop.Contracts/ServiceClients/Product/StockReleaseResult.cs`

## Related Specs
- [grpc-communication.md](../../high-level-specs/grpc-communication.md) (Section: Stock Operations)
- [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section: Cancel Order Flow)

---
## Notes
Stock can also be released via messaging (OrderCancelledEvent) as a backup mechanism.
