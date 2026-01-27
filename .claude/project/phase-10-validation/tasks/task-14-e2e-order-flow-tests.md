# Task 14: E2E Order Flow Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-14 |
| Status | ⚪ pending |
| Dependencies | task-13 |

## Objective
End-to-end tests for complete order flow across all services.

## Scope
- [ ] Test complete happy path flow
  1. Create product via Gateway → Product Service
  2. Create order via Gateway → Order Service → Product Service (gRPC)
  3. Verify order confirmed
  4. Verify stock reserved in Product Service
  5. Verify OrderConfirmed event published
  6. Verify Notification Service received event
  7. Verify email "sent" (WireMock)
- [ ] Test order rejection flow
  1. Create product with low stock
  2. Create order exceeding stock
  3. Verify order rejected
  4. Verify OrderRejected event published
  5. Verify rejection notification sent
- [ ] Test order cancellation flow
  1. Create and confirm order
  2. Cancel order
  3. Verify stock released
  4. Verify OrderCancelled event published
  5. Verify cancellation notification sent
- [ ] Test failure scenarios
  - [ ] Product Service unavailable during order creation
  - [ ] Database failure during transaction
  - [ ] Message broker unavailable (outbox queues)

## Dependencies
- Depends on: task-13
- Blocks: task-16

## Acceptance Criteria
- [ ] Complete order flow works end-to-end
- [ ] All services participate correctly
- [ ] Events propagate through message broker
- [ ] Notifications reach external service (WireMock)

## Notes
- Use async assertions with timeouts for eventual consistency
- Verify database state in each service
- Check WireMock received expected requests
- Consider test data cleanup strategy
