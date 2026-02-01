# Task 09: E2E Order Flow Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-09 |
| Status | ✅ completed |
| Dependencies | task-06, task-08 |

## Summary
End-to-end tests for complete order flows across all services - Gateway, Order, Product, Notification. Includes CorrelationId propagation verification.

## Scope

### Happy Path - Order Creation
- [x] Test create order flow (API -> stock reservation -> confirmation -> notification)
- [x] Test get order after creation
- [x] Test list orders with pagination

### Order Cancellation
- [x] Test cancel order flow (cancel -> stock release -> notification)
- [x] Test cancel already cancelled order
- [x] Test cancel non-existent order → 404

### Validation and Errors
- [x] Test invalid data returns 400 (empty items, invalid email)
- [x] Test non-existent order returns 404
- [x] Test stock unavailable error handling (insufficient stock → Rejected)
- [x] Test cancel rejected order → 400

### Gateway Routing
- [x] Test routing to correct services (Order, Product)

### CorrelationId Propagation
- [x] Test provided CorrelationId propagates across all services
- [x] Test generated CorrelationId when not provided

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md)
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md)
- → [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)

---
## Notes
- Uses E2E infrastructure from task-06 (Aspire.Hosting.Testing)
- WireMock mocks SendGrid API for email verification
- Use async polling for eventual consistency checks
- 15 E2E tests implemented in `tests/E2E.Tests/Orders/OrderFlowTests.cs`
