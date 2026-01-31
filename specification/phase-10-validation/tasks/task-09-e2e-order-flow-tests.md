# Task 09: E2E Order Flow Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-09 |
| Status | ⚪ pending |
| Dependencies | task-06, task-08 |
| Est. Tests | ~11 |

## Summary
End-to-end tests for complete order flows across all services - Gateway, Order, Product, Notification. Includes CorrelationId propagation verification.

## Scope

### Happy Path - Order Creation (~3 tests)
- [ ] `CreateOrder_HappyPath_OrderConfirmedAndNotificationSent`
  1. POST /api/orders via Gateway
  2. Order created in Order Service (status: Confirmed)
  3. Stock reserved in Product Service (gRPC)
  4. OrderConfirmedEvent published
  5. Notification Service receives event
  6. Email "sent" (verified via WireMock)

- [ ] `GetOrder_AfterCreation_ReturnsCorrectData`
  1. Create order
  2. GET /api/orders/{id}
  3. Verify all fields (status, items, total)

- [ ] `GetOrders_WithPagination_ReturnsPagedResults`
  1. Create multiple orders
  2. GET /api/orders?page=1&pageSize=2
  3. Verify pagination metadata

### Order Cancellation (~2 tests)
- [ ] `CancelOrder_HappyPath_OrderCancelledAndStockReleased`
  1. Create and confirm order
  2. POST /api/orders/{id}/cancel
  3. Order status = Cancelled
  4. Stock released in Product Service
  5. OrderCancelledEvent published
  6. Cancellation notification sent

- [ ] `CancelOrder_AlreadyCancelled_ReturnsError`
  1. Create, confirm, cancel order
  2. Try to cancel again
  3. Returns Success: false

### Validation & Errors (~3 tests)
- [ ] `CreateOrder_InvalidData_Returns400BadRequest` - validation error response
- [ ] `GetOrder_NonExistent_Returns404NotFound` - not found handling
- [ ] `CreateOrder_StockUnavailable_Returns500Error` - stock reservation failure

### Gateway Routing (~1 test)
- [ ] `Gateway_RoutesToCorrectService_OrderAndProductEndpoints`
  1. GET /api/products via Gateway → Product Service
  2. POST /api/orders via Gateway → Order Service

### CorrelationId Propagation (~2 tests)
- [ ] `Request_WithCorrelationId_PropagatedAcrossAllServices`
  1. Send request with X-Correlation-ID header
  2. Verify same ID in Order Service logs
  3. Verify same ID in Product Service (gRPC metadata)
  4. Verify same ID in Notification Service (message headers)
  5. Verify ID in response header

- [ ] `Request_WithoutCorrelationId_GatewayGeneratesOne`
  1. Send request without header
  2. Verify response contains X-Correlation-ID
  3. Verify generated ID propagated to all services

## Test Implementation Notes

```csharp
[Collection("E2E Tests")]
public class OrderFlowTests : E2ETestBase
{
    [Fact]
    public async Task CreateOrder_HappyPath_OrderConfirmedAndNotificationSent()
    {
        // Arrange
        var createOrderRequest = new
        {
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "test@example.com",
            Items = new[]
            {
                new { ProductId = _testProductId, ProductName = "Test", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        // Act
        var response = await GatewayClient.PostAsJsonAsync("/api/orders", createOrderRequest);

        // Assert - HTTP response
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<CreateOrderResult>();
        order!.Status.Should().Be("Confirmed");

        // Assert - Notification sent (WireMock)
        await WaitForConditionAsync(async () =>
        {
            var requests = WireMock.FindLogEntries(
                Request.Create().WithPath("/v3/mail/send").UsingPost()
            );
            return requests.Any();
        });
    }

    [Fact]
    public async Task Request_WithCorrelationId_PropagatedAcrossAllServices()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Headers = { { "X-Correlation-ID", correlationId } },
            Content = JsonContent.Create(CreateValidOrderRequest())
        };

        // Act
        var response = await GatewayClient.SendAsync(request);

        // Assert - Response header
        response.Headers.TryGetValues("X-Correlation-ID", out var values);
        values.Should().Contain(correlationId);

        // Assert - WireMock received same correlation ID
        var wireMockRequests = WireMock.FindLogEntries(
            Request.Create().WithPath("/v3/mail/send")
        );
        wireMockRequests.First().RequestMessage.Headers
            .Should().ContainKey("X-Correlation-ID")
            .WhoseValue.Should().Contain(correlationId);
    }
}
```

## Related Specs
- [functional-testing.md](../../high-level-specs/functional-testing.md)
- [order-service-interface.md](../../high-level-specs/order-service-interface.md)
- [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)

---
## Notes
- Uses E2E infrastructure from task-06 (Aspire.Hosting.Testing)
- WireMock mocks SendGrid API for email verification
- Use `WaitForConditionAsync` for eventual consistency checks
- CorrelationId tests verify distributed tracing works end-to-end
- Tests run against full Aspire AppHost (all services started)
