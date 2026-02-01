# Task 09: E2E Order Flow Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-09 |
| Status | ⚪ pending |
| Dependencies | task-06, task-08 |

## Summary
End-to-end tests for complete order flows across all services - Gateway, Order, Product, Notification. Includes CorrelationId propagation verification.

## Scope

### Happy Path - Order Creation
- [ ] Test create order flow (API -> stock reservation -> confirmation -> notification)
- [ ] Test get order after creation
- [ ] Test list orders with pagination

### Order Cancellation
- [ ] Test cancel order flow (cancel -> stock release -> notification)
- [ ] Test cancel already cancelled order

### Validation and Errors
- [ ] Test invalid data returns 400
- [ ] Test non-existent order returns 404
- [ ] Test stock unavailable error handling

### Gateway Routing
- [ ] Test routing to correct services (Order, Product)

### CorrelationId Propagation
- [ ] Test provided CorrelationId propagates across all services
- [ ] Test generated CorrelationId when not provided

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md)
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md)
- → [correlation-id-flow.md](../../high-level-specs/correlation-id-flow.md)

---
## Notes
- Uses E2E infrastructure from task-06 (Aspire.Hosting.Testing)
- WireMock mocks SendGrid API for email verification
- Use async polling for eventual consistency checks
