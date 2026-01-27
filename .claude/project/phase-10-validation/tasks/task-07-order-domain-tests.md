# Task 07: Order Domain Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ⚪ pending |
| Dependencies | task-06 |

## Objective
Comprehensive unit tests for Order Service domain entities and lifecycle management.

## Scope
- [ ] Test `OrderEntity` state transitions
  - [ ] Create → Confirmed (happy path)
  - [ ] Create → Rejected (stock unavailable)
  - [ ] Confirmed → Cancelled
  - [ ] Invalid transitions throw exceptions
- [ ] Test `OrderItem` value object
  - [ ] Creation and validation
  - [ ] Price calculation
- [ ] Test `OrderStatus` transitions
  - [ ] All valid transitions
  - [ ] All invalid transitions
- [ ] Test command handlers
  - [ ] CreateOrderCommandHandler (with mocked IProductServiceClient)
    - [ ] Stock available → Confirmed
    - [ ] Stock unavailable → Rejected
    - [ ] Product service failure → proper error handling
  - [ ] CancelOrderCommandHandler
    - [ ] Confirmed order → Cancelled + stock released
    - [ ] Already cancelled → error
  - [ ] GetOrderQueryHandler
- [ ] Test validators
  - [ ] CreateOrderCommandValidator (representative test)
- [ ] Test Outbox integration
  - [ ] OrderConfirmed event queued on confirmation
  - [ ] OrderCancelled event queued on cancellation

## Dependencies
- Depends on: task-06
- Blocks: task-12

## Acceptance Criteria
- [ ] All order state transitions tested (valid + invalid)
- [ ] Command handlers tested with all edge cases
- [ ] Outbox event publishing verified

## Notes
- Mock IProductServiceClient for stock reservation calls
- Mock IOutboxRepository to verify events are queued
- Use InMemory DbContext for handler tests
- Order lifecycle is critical - be thorough
